using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Flashcard;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IFlashcardAdminService
{
    Task<PagedResult<DeckListViewModel>> GetDecksPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<DeckDetailViewModel?> GetDeckDetailAsync(int id, CancellationToken ct = default);
    Task<DeckFormViewModel?> GetDeckForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateDeckAsync(DeckFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> UpdateDeckAsync(DeckFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> DeleteDeckAsync(int id, CancellationToken ct = default);

    Task<CardFormViewModel?> GetCardForEditAsync(int cardId, CancellationToken ct = default);
    Task<ServiceResult<int>> AddCardAsync(CardFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> UpdateCardAsync(CardFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> DeleteCardAsync(int cardId, int deckId, CancellationToken ct = default);
}
