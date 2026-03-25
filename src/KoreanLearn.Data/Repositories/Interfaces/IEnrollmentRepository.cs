using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>選課紀錄 Repository 介面，提供選課與存取權限相關查詢</summary>
public interface IEnrollmentRepository : IRepository<Enrollment>
{
    /// <summary>取得指定使用者與課程的選課紀錄</summary>
    Task<Enrollment?> GetByUserAndCourseAsync(string userId, int courseId, CancellationToken ct = default);

    /// <summary>取得使用者的所有選課紀錄</summary>
    Task<IReadOnlyList<Enrollment>> GetByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>檢查使用者是否已選修該課程</summary>
    Task<bool> IsEnrolledAsync(string userId, int courseId, CancellationToken ct = default);

    /// <summary>檢查使用者是否有課程存取權（Active/Completed Enrollment 或有效訂閱）</summary>
    Task<bool> HasActiveAccessAsync(string userId, int courseId, CancellationToken ct = default);

    /// <summary>依多個課程 ID 統計選課人數總和</summary>
    Task<int> CountByCourseIdsAsync(IEnumerable<int> courseIds, CancellationToken ct = default);
}
