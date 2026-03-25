using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class PronunciationRepository(ApplicationDbContext db) : Repository<PronunciationExercise>(db), IPronunciationRepository
{
    public async Task<IReadOnlyList<PronunciationExercise>> GetByLessonIdAsync(
        int lessonId, CancellationToken ct = default)
        => await DbSet.Where(p => p.LessonId == lessonId).OrderBy(p => p.Id).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<PronunciationExercise>> GetAllWithAttemptsAsync(
        string userId, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Attempts.Where(a => a.UserId == userId))
            .OrderBy(p => p.Id)
            .ToListAsync(ct).ConfigureAwait(false);
}
