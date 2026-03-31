using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class QuizRepository(ApplicationDbContext db) : Repository<Quiz>(db), IQuizRepository
{
    public async Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken ct = default)
        => await DbSet
            .Include(q => q.Questions.OrderBy(qq => qq.SortOrder))
                .ThenInclude(q => q.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(q => q.Id == id, ct).ConfigureAwait(false);

    public async Task<Quiz?> GetByLessonIdAsync(int lessonId, CancellationToken ct = default)
        => await DbSet
            .Include(q => q.Questions.OrderBy(qq => qq.SortOrder))
                .ThenInclude(q => q.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(q => q.LessonId == lessonId, ct).ConfigureAwait(false);

    public async Task<QuizQuestion?> GetQuestionByIdAsync(int questionId, CancellationToken ct = default)
        => await Db.Set<QuizQuestion>()
            .Include(q => q.Options.OrderBy(o => o.SortOrder))
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct).ConfigureAwait(false);
}
