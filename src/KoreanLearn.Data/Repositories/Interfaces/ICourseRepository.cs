using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);
    Task<PagedResult<Course>> SearchAsync(string? keyword, int page, int pageSize, CancellationToken ct = default);
    Task<Course?> GetWithSectionsAndLessonsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Course>> GetPublishedAsync(CancellationToken ct = default);
}
