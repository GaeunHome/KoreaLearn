using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>
/// 前台字卡學習業務邏輯實作，使用 SM-2 間隔重複演算法排程複習。
/// SM-2 演算法根據使用者的回答品質（0-5）動態調整每張卡片的複習間隔與難度因子。
/// </summary>
public class FlashcardLearnService(
    IUnitOfWork uow,
    ILogger<FlashcardLearnService> logger) : IFlashcardLearnService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<FlashcardDeckListViewModel>> GetDecksForStudyAsync(
        string userId, CancellationToken ct = default)
    {
        logger.LogDebug("取得學習牌組列表 | UserId={UserId}", userId);
        var decks = await uow.FlashcardDecks.GetAllAsync(ct).ConfigureAwait(false);
        var result = new List<FlashcardDeckListViewModel>();

        foreach (var deck in decks)
        {
            var full = await uow.FlashcardDecks.GetWithCardsAsync(deck.Id, ct).ConfigureAwait(false);
            var dueCount = (await uow.FlashcardLogs.GetDueCardsAsync(userId, deck.Id, ct).ConfigureAwait(false)).Count;
            var newCount = full?.Flashcards.Count ?? 0;

            result.Add(new FlashcardDeckListViewModel
            {
                Id = deck.Id,
                Title = deck.Title,
                Description = deck.Description,
                CardCount = full?.Flashcards.Count ?? 0,
                DueCount = dueCount
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<FlashcardStudyViewModel?> GetStudySessionAsync(
        int deckId, string userId, CancellationToken ct = default)
    {
        logger.LogInformation("開始學習 Session | UserId={UserId} | DeckId={DeckId}", userId, deckId);
        var deck = await uow.FlashcardDecks.GetWithCardsAsync(deckId, ct).ConfigureAwait(false);
        if (deck is null)
        {
            logger.LogWarning("學習 Session 失敗：牌組不存在 | DeckId={DeckId}", deckId);
            return null;
        }

        var dueLogs = await uow.FlashcardLogs.GetDueCardsAsync(userId, deckId, ct).ConfigureAwait(false);
        var dueCardIds = dueLogs.Select(l => l.FlashcardId).ToHashSet();

        var allLogs = await uow.FlashcardLogs.GetByUserAndDeckAsync(userId, deckId, ct).ConfigureAwait(false);
        var seenCardIds = allLogs.Select(l => l.FlashcardId).ToHashSet();
        var newCardIds = deck.Flashcards.Select(c => c.Id).Except(seenCardIds).ToHashSet();

        var studyCards = BuildStudyCardList(deck.Flashcards, dueCardIds, newCardIds);

        logger.LogInformation("學習 Session 準備完成 | DeckId={DeckId} | StudyCards={StudyCount} | DueCards={DueCount} | NewCards={NewCount}",
            deck.Id, studyCards.Count, dueCardIds.Count, newCardIds.Count);

        return new FlashcardStudyViewModel
        {
            DeckId = deck.Id,
            DeckTitle = deck.Title,
            Cards = studyCards,
            TotalCards = deck.Flashcards.Count,
            DueCards = dueCardIds.Count,
            NewCards = newCardIds.Count
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ReviewCardAsync(
        string userId, int cardId, int quality, CancellationToken ct = default)
    {
        quality = Math.Clamp(quality, 0, 5);

        if (cardId <= 0)
        {
            logger.LogWarning("複習字卡 Id 無效 | CardId={CardId} | UserId={UserId}", cardId, userId);
            return ServiceResult.Failure("字卡 Id 無效");
        }

        var log = await uow.FlashcardLogs.GetByUserAndCardAsync(userId, cardId, ct).ConfigureAwait(false);

        var isNew = log is null;
        if (isNew)
        {
            log = new FlashcardLog
            {
                UserId = userId,
                FlashcardId = cardId,
                EaseFactor = 2.5,
                Interval = 0,
                Repetition = 0
            };
        }

        ApplySm2Algorithm(log!, quality);

        if (isNew)
            await uow.FlashcardLogs.AddAsync(log!, ct).ConfigureAwait(false);
        else
            uow.FlashcardLogs.Update(log!);

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogDebug("SM-2 複習 | CardId={CardId} | Q={Quality} | EF={EF:F2} | Interval={Interval}d",
            cardId, quality, log!.EaseFactor, log.Interval);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<int> GetDueCardCountAsync(string userId, CancellationToken ct = default)
    {
        var count = await uow.FlashcardLogs.CountDueForUserAsync(userId, ct).ConfigureAwait(false);
        return count;
    }

    /// <summary>組建學習卡片清單：到期卡片優先，其次新卡片，每次最多 20 張</summary>
    private static List<FlashcardItemViewModel> BuildStudyCardList(
        ICollection<Flashcard> allCards, HashSet<int> dueCardIds, HashSet<int> newCardIds)
    {
        var studyCards = new List<FlashcardItemViewModel>();

        foreach (var card in allCards.Where(c => dueCardIds.Contains(c.Id)))
        {
            studyCards.Add(MapToItem(card, isNew: false));
        }

        foreach (var card in allCards.Where(c => newCardIds.Contains(c.Id)).Take(20 - studyCards.Count))
        {
            if (studyCards.Count >= 20) break;
            studyCards.Add(MapToItem(card, isNew: true));
        }

        return studyCards;
    }

    private static FlashcardItemViewModel MapToItem(Flashcard card, bool isNew) => new()
    {
        CardId = card.Id,
        Korean = card.Korean,
        Chinese = card.Chinese,
        Romanization = card.Romanization,
        ExampleSentence = card.ExampleSentence,
        IsNew = isNew
    };

    /// <summary>
    /// SM-2 間隔重複演算法：根據回答品質更新重複次數、間隔、難度因子與下次複習日期。
    /// quality >= 3（正確）：累計重複次數並延長間隔；quality &lt; 3（錯誤）：重設為 1 天。
    /// </summary>
    private static void ApplySm2Algorithm(FlashcardLog log, int quality)
    {
        if (quality >= 3)
        {
            log.Repetition++;
            log.Interval = log.Repetition switch
            {
                1 => 1,
                2 => 6,
                _ => (int)Math.Round(log.Interval * log.EaseFactor)
            };
        }
        else
        {
            log.Repetition = 0;
            log.Interval = 1;
        }

        log.EaseFactor = Math.Max(1.3,
            log.EaseFactor + 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
        log.Quality = quality;
        log.NextReviewDate = DateTime.UtcNow.AddDays(log.Interval);
    }
}
