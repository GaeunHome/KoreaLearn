using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>公告 Repository 介面</summary>
public interface IAnnouncementRepository : IRepository<Announcement>
{
    /// <summary>取得目前有效的公告（IsActive + 日期範圍內），按置頂 → 排序 → 建立時間排列</summary>
    Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>取得前 N 筆有效公告（首頁用）</summary>
    Task<IReadOnlyList<Announcement>> GetLatestActiveAsync(int count, CancellationToken ct = default);

    /// <summary>取得已發布公告的分頁結果（前台列表用）</summary>
    Task<PagedResult<Announcement>> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得公告含附件（前台詳情用），僅限已發布</summary>
    Task<Announcement?> GetPublishedWithAttachmentsAsync(int id, CancellationToken ct = default);

    /// <summary>取得所有公告含軟刪除（後台管理用）</summary>
    Task<IReadOnlyList<Announcement>> GetAllIncludeDeletedAsync(CancellationToken ct = default);

    /// <summary>取得公告含附件（後台編輯用，含軟刪除）</summary>
    Task<Announcement?> GetWithAttachmentsIncludeDeletedAsync(int id, CancellationToken ct = default);

    /// <summary>取得目前最大排序號</summary>
    Task<int> GetMaxSortOrderAsync(CancellationToken ct = default);
}
