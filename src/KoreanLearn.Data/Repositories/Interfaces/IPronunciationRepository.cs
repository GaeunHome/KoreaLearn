using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IPronunciationRepository : IRepository<PronunciationExercise>
{
    Task<IReadOnlyList<PronunciationExercise>> GetByLessonIdAsync(int lessonId, CancellationToken ct = default);
    Task<IReadOnlyList<PronunciationExercise>> GetAllWithAttemptsAsync(string userId, CancellationToken ct = default);
}
