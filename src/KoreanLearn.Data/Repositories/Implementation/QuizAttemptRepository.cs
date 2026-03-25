using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class QuizAttemptRepository(ApplicationDbContext db) : Repository<QuizAttempt>(db), IQuizAttemptRepository
{
    public async Task<QuizAttempt?> GetWithAnswersAsync(int id, CancellationToken ct = default)
        => await DbSet
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Question)
                    .ThenInclude(q => q.Options)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOption)
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(a => a.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<QuizAttempt>> GetByUserAndQuizAsync(
        string userId, int quizId, CancellationToken ct = default)
        => await DbSet
            .Where(a => a.UserId == userId && a.QuizId == quizId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(ct).ConfigureAwait(false);
}
