using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class FlashcardLearnService(
    IUnitOfWork uow,
    ILogger<FlashcardLearnService> logger) : IFlashcardLearnService
{
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

    public async Task<FlashcardStudyViewModel?> GetStudySessionAsync(
        int deckId, string userId, CancellationToken ct = default)
    {
        var deck = await uow.FlashcardDecks.GetWithCardsAsync(deckId, ct).ConfigureAwait(false);
        if (deck is null) return null;

        // Get due cards (cards with expired review date)
        var dueLogs = await uow.FlashcardLogs.GetDueCardsAsync(userId, deckId, ct).ConfigureAwait(false);
        var dueCardIds = dueLogs.Select(l => l.FlashcardId).ToHashSet();

        // 批次載入使用者的所有學習紀錄，避免 N+1 查詢
        var allLogs = await uow.FlashcardLogs.GetByUserAndDeckAsync(userId, deckId, ct).ConfigureAwait(false);
        var seenCardIds = allLogs.Select(l => l.FlashcardId).ToHashSet();
        var newCardIds = deck.Flashcards.Select(c => c.Id).Except(seenCardIds).ToHashSet();

        // Study session: due cards first, then new cards (max 20 per session)
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

    public async Task<ServiceResult> ReviewCardAsync(
        string userId, int cardId, int quality, CancellationToken ct = default)
    {
        quality = Math.Clamp(quality, 0, 5);
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

        // SM-2 Algorithm
        if (quality >= 3)
        {
            log!.Repetition++;
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
