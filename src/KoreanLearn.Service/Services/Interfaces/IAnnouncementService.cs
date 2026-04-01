using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Announcement;
using KoreanLearn.Service.ViewModels.Announcement;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>公告業務邏輯介面</summary>
public interface IAnnouncementService
{
    // ── 前台 ──

    /// <summary>取得已發布公告分頁列表</summary>
    Task<PagedResult<AnnouncementCardViewModel>> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得公告詳情（前台，僅限已發布）</summary>
    Task<AnnouncementDetailViewModel?> GetDetailAsync(int id, CancellationToken ct = default);

    // ── 後台 ──

    /// <summary>取得所有公告列表（含軟刪除）</summary>
    Task<IReadOnlyList<AnnouncementListItemViewModel>> GetAllForAdminAsync(CancellationToken ct = default);

    /// <summary>取得公告編輯表單資料</summary>
    Task<AnnouncementFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default);

    /// <summary>建立公告</summary>
    Task<ServiceResult<int>> CreateAsync(AnnouncementFormViewModel vm, IReadOnlyList<(string fileUrl, string fileName, long fileSize)>? attachments, CancellationToken ct = default);

    /// <summary>更新公告</summary>
    Task<ServiceResult> UpdateAsync(AnnouncementFormViewModel vm, IReadOnlyList<(string fileUrl, string fileName, long fileSize)>? newAttachments, CancellationToken ct = default);

    /// <summary>軟刪除公告</summary>
    Task<ServiceResult> SoftDeleteAsync(int id, CancellationToken ct = default);

    /// <summary>復原軟刪除的公告</summary>
    Task<ServiceResult> RestoreAsync(int id, CancellationToken ct = default);

    /// <summary>切換置頂狀態</summary>
    Task<ServiceResult> TogglePinAsync(int id, CancellationToken ct = default);

    /// <summary>拖曳排序（更新所有項目的 SortOrder）</summary>
    Task<ServiceResult> ReorderAsync(IReadOnlyList<int> orderedIds, CancellationToken ct = default);
}
