using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<Enrollment?> GetByUserAndCourseAsync(string userId, int courseId, CancellationToken ct = default);
    Task<IReadOnlyList<Enrollment>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<bool> IsEnrolledAsync(string userId, int courseId, CancellationToken ct = default);
}
