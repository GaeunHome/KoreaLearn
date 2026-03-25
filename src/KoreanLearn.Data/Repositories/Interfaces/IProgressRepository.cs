using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IProgressRepository : IRepository<Progress>
{
    Task<Progress?> GetByUserAndLessonAsync(string userId, int lessonId, CancellationToken ct = default);
    Task<IReadOnlyList<Progress>> GetByUserAndCourseAsync(string userId, int courseId, CancellationToken ct = default);
}
