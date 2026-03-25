using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Pronunciation;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>發音練習業務邏輯介面（後台管理 + 前台練習與錄音上傳）</summary>
public interface IPronunciationService
{
    /// <summary>取得發音練習分頁列表（後台管理用）</summary>
    Task<PagedResult<PronunciationListViewModel>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得發音練習編輯表單資料</summary>
    Task<PronunciationFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default);

    /// <summary>建立新發音練習，回傳 ID</summary>
    Task<ServiceResult<int>> CreateAsync(PronunciationFormViewModel vm, string audioUrl, CancellationToken ct = default);

    /// <summary>更新發音練習資訊</summary>
    Task<ServiceResult> UpdateAsync(PronunciationFormViewModel vm, string? newAudioUrl, CancellationToken ct = default);

    /// <summary>軟刪除發音練習</summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>取得所有發音練習（前台練習頁面用）</summary>
    Task<IReadOnlyList<PronunciationListViewModel>> GetAllForPracticeAsync(CancellationToken ct = default);

    /// <summary>儲存學生的錄音練習紀錄</summary>
    Task<ServiceResult> SaveAttemptAsync(string userId, int exerciseId, string recordingUrl, CancellationToken ct = default);
}
