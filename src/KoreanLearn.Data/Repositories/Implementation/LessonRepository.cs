using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class LessonRepository(ApplicationDbContext db) : Repository<Lesson>(db), ILessonRepository
{
    public async Task<IReadOnlyList<Lesson>> GetBySectionIdAsync(int sectionId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(l => l.SectionId == sectionId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<Lesson?> GetWithQuizAsync(int id, CancellationToken ct = default)
        => await DbSet
            .Include(l => l.Quiz)
                .ThenInclude(q => q!.Questions.OrderBy(qq => qq.SortOrder))
                    .ThenInclude(q => q.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(l => l.Id == id, ct).ConfigureAwait(false);
}
