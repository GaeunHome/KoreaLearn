using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<Enrollment?> GetByUserAndCourseAsync(string userId, int courseId, CancellationToken ct = default);
    Task<IReadOnlyList<Enrollment>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<bool> IsEnrolledAsync(string userId, int courseId, CancellationToken ct = default);
    /// <summary>檢查用戶是否有課程存取權（Active/Completed Enrollment 或有效訂閱）</summary>
    Task<bool> HasActiveAccessAsync(string userId, int courseId, CancellationToken ct = default);
    Task<int> CountByCourseIdsAsync(IEnumerable<int> courseIds, CancellationToken ct = default);
}
