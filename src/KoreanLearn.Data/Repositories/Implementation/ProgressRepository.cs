using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class ProgressRepository(ApplicationDbContext db) : Repository<Progress>(db), IProgressRepository
{
    public async Task<Progress?> GetByUserAndLessonAsync(string userId, int lessonId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Progress>> GetByUserAndCourseAsync(string userId, int courseId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(p => p.Lesson)
                .ThenInclude(l => l.Section)
            .Where(p => p.UserId == userId && p.Lesson.Section.CourseId == courseId)
            .ToListAsync(ct).ConfigureAwait(false);
}
