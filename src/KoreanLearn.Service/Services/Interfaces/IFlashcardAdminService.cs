using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Flashcard;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>後台字卡管理業務邏輯介面（涵蓋牌組與字卡的 CRUD）</summary>
public interface IFlashcardAdminService
{
    /// <summary>取得牌組分頁列表</summary>
    Task<PagedResult<DeckListViewModel>> GetDecksPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得牌組詳情（含所有字卡）</summary>
    Task<DeckDetailViewModel?> GetDeckDetailAsync(int id, CancellationToken ct = default);

    /// <summary>取得牌組編輯表單資料</summary>
    Task<DeckFormViewModel?> GetDeckForEditAsync(int id, CancellationToken ct = default);

    /// <summary>建立新牌組，回傳牌組 ID</summary>
    Task<ServiceResult<int>> CreateDeckAsync(DeckFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新牌組資訊</summary>
    Task<ServiceResult> UpdateDeckAsync(DeckFormViewModel vm, CancellationToken ct = default);

    /// <summary>軟刪除牌組</summary>
    Task<ServiceResult> DeleteDeckAsync(int id, CancellationToken ct = default);

    /// <summary>取得字卡編輯表單資料</summary>
    Task<CardFormViewModel?> GetCardForEditAsync(int cardId, CancellationToken ct = default);

    /// <summary>新增字卡至指定牌組</summary>
    Task<ServiceResult<int>> AddCardAsync(CardFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新字卡內容</summary>
    Task<ServiceResult> UpdateCardAsync(CardFormViewModel vm, CancellationToken ct = default);

    /// <summary>刪除字卡</summary>
    Task<ServiceResult> DeleteCardAsync(int cardId, int deckId, CancellationToken ct = default);
}
