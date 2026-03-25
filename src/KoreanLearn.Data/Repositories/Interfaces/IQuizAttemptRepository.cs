using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IQuizAttemptRepository : IRepository<QuizAttempt>
{
    Task<QuizAttempt?> GetWithAnswersAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<QuizAttempt>> GetByUserAndQuizAsync(string userId, int quizId, CancellationToken ct = default);
}
