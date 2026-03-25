using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class SectionRepository(ApplicationDbContext db) : Repository<Section>(db), ISectionRepository
{
    public async Task<IReadOnlyList<Section>> GetByCourseIdAsync(int courseId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct).ConfigureAwait(false);
}
