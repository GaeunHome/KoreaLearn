using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
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
        => await DbSet.AnyAsync(e => e.UserId == userId && e.CourseId == courseId, ct).ConfigureAwait(false);
}
