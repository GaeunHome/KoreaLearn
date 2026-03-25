using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Flashcard;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>後台字卡管理業務邏輯實作，處理牌組與字卡（韓文/中文/羅馬拼音）的 CRUD</summary>
public class FlashcardAdminService(
    IUnitOfWork uow,
    ILogger<FlashcardAdminService> logger) : IFlashcardAdminService
{
    /// <inheritdoc />
    public async Task<PagedResult<DeckListViewModel>> GetDecksPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.FlashcardDecks.GetPagedAsync(page, pageSize, ct).ConfigureAwait(false);
        var items = new List<DeckListViewModel>();
        foreach (var deck in result.Items)
        {
            var full = await uow.FlashcardDecks.GetWithCardsAsync(deck.Id, ct).ConfigureAwait(false);
            items.Add(new DeckListViewModel
            {
                Id = deck.Id,
                Title = deck.Title,
                Description = deck.Description,
                CourseId = deck.CourseId,
                CardCount = full?.Flashcards.Count ?? 0,
                CreatedAt = deck.CreatedAt
            });
        }
        return new PagedResult<DeckListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<DeckDetailViewModel?> GetDeckDetailAsync(int id, CancellationToken ct = default)
    {
        var deck = await uow.FlashcardDecks.GetWithCardsAsync(id, ct).ConfigureAwait(false);
        if (deck is null) return null;

        return new DeckDetailViewModel
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            CourseId = deck.CourseId,
            Cards = deck.Flashcards.Select(c => new CardViewModel
            {
                Id = c.Id,
                Korean = c.Korean,
                Chinese = c.Chinese,
                Romanization = c.Romanization,
                ExampleSentence = c.ExampleSentence,
                SortOrder = c.SortOrder
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<DeckFormViewModel?> GetDeckForEditAsync(int id, CancellationToken ct = default)
    {
        var deck = await uow.FlashcardDecks.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (deck is null) return null;
        return new DeckFormViewModel
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            CourseId = deck.CourseId
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateDeckAsync(DeckFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("建立字卡牌組 | Title={Title}", vm.Title);
        var deck = new FlashcardDeck
        {
            Title = vm.Title,
            Description = vm.Description,
            CourseId = vm.CourseId
        };
        await uow.FlashcardDecks.AddAsync(deck, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("牌組建立成功 | DeckId={DeckId}", deck.Id);
        return ServiceResult<int>.Success(deck.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateDeckAsync(DeckFormViewModel vm, CancellationToken ct = default)
    {
        var deck = await uow.FlashcardDecks.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (deck is null) return ServiceResult.Failure("牌組不存在");

        deck.Title = vm.Title;
        deck.Description = vm.Description;
        deck.CourseId = vm.CourseId;
        uow.FlashcardDecks.Update(deck);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteDeckAsync(int id, CancellationToken ct = default)
    {
        var deck = await uow.FlashcardDecks.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (deck is null) return ServiceResult.Failure("牌組不存在");
        uow.FlashcardDecks.Remove(deck);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("牌組刪除成功（軟刪除）| DeckId={DeckId}", id);
        return ServiceResult.Success();
    }

    // ── 字卡管理 ──────────────────────────────────────

    /// <inheritdoc />
    public async Task<CardFormViewModel?> GetCardForEditAsync(int cardId, CancellationToken ct = default)
    {
        var card = await uow.FlashcardDecks.GetCardByIdAsync(cardId, ct).ConfigureAwait(false);
        if (card is null) return null;

        return new CardFormViewModel
        {
            Id = card.Id,
            DeckId = card.DeckId,
            Korean = card.Korean,
            Chinese = card.Chinese,
            Romanization = card.Romanization,
            ExampleSentence = card.ExampleSentence,
            SortOrder = card.SortOrder,
            DeckTitle = card.Deck?.Title
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> AddCardAsync(CardFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("新增字卡 | DeckId={DeckId} | Korean={Korean}", vm.DeckId, vm.Korean);
        var deck = await uow.FlashcardDecks.GetWithCardsAsync(vm.DeckId, ct).ConfigureAwait(false);
        if (deck is null) return ServiceResult<int>.Failure("牌組不存在");

        var card = new Flashcard
        {
            DeckId = vm.DeckId,
            Korean = vm.Korean,
            Chinese = vm.Chinese,
            Romanization = vm.Romanization,
            ExampleSentence = vm.ExampleSentence,
            SortOrder = vm.SortOrder
        };
        deck.Flashcards.Add(card);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("字卡新增成功 | CardId={CardId}", card.Id);
        return ServiceResult<int>.Success(card.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateCardAsync(CardFormViewModel vm, CancellationToken ct = default)
    {
        var deck = await uow.FlashcardDecks.GetWithCardsAsync(vm.DeckId, ct).ConfigureAwait(false);
        var card = deck?.Flashcards.FirstOrDefault(c => c.Id == vm.Id);
        if (card is null) return ServiceResult.Failure("字卡不存在");

        card.Korean = vm.Korean;
        card.Chinese = vm.Chinese;
        card.Romanization = vm.Romanization;
        card.ExampleSentence = vm.ExampleSentence;
        card.SortOrder = vm.SortOrder;
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteCardAsync(int cardId, int deckId, CancellationToken ct = default)
    {
        var deck = await uow.FlashcardDecks.GetWithCardsAsync(deckId, ct).ConfigureAwait(false);
        var card = deck?.Flashcards.FirstOrDefault(c => c.Id == cardId);
        if (card is null) return ServiceResult.Failure("字卡不存在");

        deck!.Flashcards.Remove(card);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("字卡刪除成功 | CardId={CardId}", cardId);
        return ServiceResult.Success();
    }
}
