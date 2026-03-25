using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface IDiscussionRepository : IRepository<Discussion>
{
    Task<Discussion?> GetWithRepliesAsync(int id, CancellationToken ct = default);
    Task<PagedResult<Discussion>> GetByCourseIdAsync(int courseId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Discussion>> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
