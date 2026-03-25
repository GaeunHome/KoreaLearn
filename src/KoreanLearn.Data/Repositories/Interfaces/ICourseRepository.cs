using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>課程 Repository 介面，提供課程相關的資料查詢</summary>
public interface ICourseRepository : IRepository<Course>
{
    /// <summary>依標題檢查課程是否已存在</summary>
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);

    /// <summary>依關鍵字搜尋課程（分頁）</summary>
    Task<PagedResult<Course>> SearchAsync(string? keyword, int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得課程及其章節與單元（含 Include）</summary>
    Task<Course?> GetWithSectionsAndLessonsAsync(int id, CancellationToken ct = default);

    /// <summary>取得所有已上架的課程</summary>
    Task<IReadOnlyList<Course>> GetPublishedAsync(CancellationToken ct = default);

    /// <summary>依教師 ID 取得課程（分頁）</summary>
    Task<PagedResult<Course>> GetByTeacherIdPagedAsync(string teacherId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>檢查課程是否屬於指定教師</summary>
    Task<bool> IsOwnedByTeacherAsync(int courseId, string teacherId, CancellationToken ct = default);
}
