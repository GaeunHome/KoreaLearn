using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class EnrollmentRepository(ApplicationDbContext db) : Repository<Enrollment>(db), IEnrollmentRepository
{
    public async Task<Enrollment?> GetByUserAndCourseAsync(string userId, int courseId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Enrollment>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<bool> IsEnrolledAsync(string userId, int courseId, CancellationToken ct = default)
        => await DbSet.AnyAsync(
            e => e.UserId == userId && e.CourseId == courseId
                && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed),
            ct).ConfigureAwait(false);

    public async Task<bool> HasActiveAccessAsync(string userId, int courseId, CancellationToken ct = default)
    {
        // 檢查 Enrollment（Active 或 Completed）
        var enrolled = await IsEnrolledAsync(userId, courseId, ct).ConfigureAwait(false);
        if (enrolled) return true;

        // 檢查有效訂閱（訂閱用戶可存取所有課程）
        var hasSubscription = await Db.Set<UserSubscription>()
            .AnyAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow, ct)
            .ConfigureAwait(false);
        return hasSubscription;
    }

    public async Task<int> CountByCourseIdsAsync(IEnumerable<int> courseIds, CancellationToken ct = default)
    {
        var ids = courseIds.ToList();
        return await DbSet.CountAsync(e => ids.Contains(e.CourseId), ct).ConfigureAwait(false);
    }
}
