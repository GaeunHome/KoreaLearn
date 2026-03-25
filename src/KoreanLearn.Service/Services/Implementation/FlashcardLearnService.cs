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
        var deck = await uow.FlashcardDecks.GetWithCardsAsync(deckId, ct).ConfigureAwait(false);
        if (deck is null) return null;

        // 取得已到期需複習的卡片
        var dueLogs = await uow.FlashcardLogs.GetDueCardsAsync(userId, deckId, ct).ConfigureAwait(false);
        var dueCardIds = dueLogs.Select(l => l.FlashcardId).ToHashSet();

        // 批次載入使用者的所有學習紀錄，避免 N+1 查詢
        var allLogs = await uow.FlashcardLogs.GetByUserAndDeckAsync(userId, deckId, ct).ConfigureAwait(false);
        var seenCardIds = allLogs.Select(l => l.FlashcardId).ToHashSet();
        var newCardIds = deck.Flashcards.Select(c => c.Id).Except(seenCardIds).ToHashSet();

        // 學習順序：到期卡片優先，其次新卡片，每次最多 20 張
        var studyCards = new List<FlashcardItemViewModel>();

        foreach (var card in deck.Flashcards.Where(c => dueCardIds.Contains(c.Id)))
        {
            studyCards.Add(new FlashcardItemViewModel
            {
                CardId = card.Id, Korean = card.Korean, Chinese = card.Chinese,
                Romanization = card.Romanization, ExampleSentence = card.ExampleSentence
            });
        }

        foreach (var card in deck.Flashcards.Where(c => newCardIds.Contains(c.Id)).Take(20 - studyCards.Count))
        {
            if (studyCards.Count >= 20) break;
            studyCards.Add(new FlashcardItemViewModel
            {
                CardId = card.Id, Korean = card.Korean, Chinese = card.Chinese,
                Romanization = card.Romanization, ExampleSentence = card.ExampleSentence,
                IsNew = true
            });
        }

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
        // quality 範圍限制在 0-5（SM-2 標準範圍）
        quality = Math.Clamp(quality, 0, 5);
        var log = await uow.FlashcardLogs.GetByUserAndCardAsync(userId, cardId, ct).ConfigureAwait(false);

        var isNew = log is null;
        if (isNew)
        {
            // 新卡片初始化：EF=2.5、間隔=0、重複次數=0
            log = new FlashcardLog
            {
                UserId = userId,
                FlashcardId = cardId,
                EaseFactor = 2.5,
                Interval = 0,
                Repetition = 0
            };
        }

        // ═══ SM-2 間隔重複演算法 ═══
        //
        // 步驟 1：根據回答品質決定是否重設重複次數
        //   - quality >= 3（正確回答）：增加重複次數，延長間隔
        //   - quality < 3（錯誤回答）：重設重複次數為 0，間隔歸 1 天
        //
        // 步驟 2：計算新的複習間隔
        //   - 第 1 次正確：間隔 = 1 天
        //   - 第 2 次正確：間隔 = 6 天
        //   - 之後：間隔 = 前次間隔 * 難度因子 (EF)
        //
        // 步驟 3：更新難度因子 (EF)
        //   - EF' = EF + (0.1 - (5-q) * (0.08 + (5-q) * 0.02))
        //   - EF 最小值為 1.3（避免間隔縮得太短）
        //
        // 步驟 4：設定下次複習日期 = 今天 + 間隔天數

        if (quality >= 3)
        {
            // 回答正確：累計重複次數，按規則延長間隔
            log!.Repetition++;
            log.Interval = log.Repetition switch
            {
                1 => 1,   // 第 1 次正確：明天複習
                2 => 6,   // 第 2 次正確：6 天後複習
                _ => (int)Math.Round(log.Interval * log.EaseFactor) // 之後按 EF 倍率延長
            };
        }
        else
        {
            // 回答錯誤：重頭開始，明天再複習
            log.Repetition = 0;
            log.Interval = 1;
        }

        // 更新難度因子（EF），最低不小於 1.3
        log.EaseFactor = Math.Max(1.3,
            log.EaseFactor + 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
        log.Quality = quality;
        log.NextReviewDate = DateTime.UtcNow.AddDays(log.Interval);

        if (isNew)
            await uow.FlashcardLogs.AddAsync(log, ct).ConfigureAwait(false);
        else
            uow.FlashcardLogs.Update(log);

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogDebug("SM-2 複習 | CardId={CardId} | Q={Quality} | EF={EF:F2} | Interval={Interval}d",
            cardId, quality, log.EaseFactor, log.Interval);

        return ServiceResult.Success();
    }
}
