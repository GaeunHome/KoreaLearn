using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken ct = default);
    Task<Quiz?> GetByLessonIdAsync(int lessonId, CancellationToken ct = default);
}
