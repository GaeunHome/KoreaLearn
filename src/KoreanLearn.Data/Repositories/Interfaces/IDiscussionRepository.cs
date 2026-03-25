using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>討論區 Repository 介面，提供討論主題與回覆的查詢</summary>
public interface IDiscussionRepository : IRepository<Discussion>
{
    /// <summary>取得討論主題及其所有回覆</summary>
    Task<Discussion?> GetWithRepliesAsync(int id, CancellationToken ct = default);

    /// <summary>依課程 ID 取得討論列表（分頁）</summary>
    Task<PagedResult<Discussion>> GetByCourseIdAsync(int courseId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得所有討論列表（分頁，用於後台管理）</summary>
    Task<PagedResult<Discussion>> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
