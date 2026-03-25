using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<IReadOnlyList<Lesson>> GetBySectionIdAsync(int sectionId, CancellationToken ct = default);
    Task<Lesson?> GetWithQuizAsync(int id, CancellationToken ct = default);
}
