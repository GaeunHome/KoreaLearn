using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class LessonAttachmentRepository(ApplicationDbContext db)
    : Repository<LessonAttachment>(db), ILessonAttachmentRepository
{
    public async Task<IReadOnlyList<LessonAttachment>> GetByLessonIdAsync(
        int lessonId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(a => a.LessonId == lessonId)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(ct).ConfigureAwait(false);
}
