using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using KoreanLearn.Library.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class DiscussionRepository(ApplicationDbContext db) : Repository<Discussion>(db), IDiscussionRepository
{
    public async Task<Discussion?> GetWithRepliesAsync(int id, CancellationToken ct = default)
        => await DbSet
            .Include(d => d.User)
            .Include(d => d.Replies.OrderBy(r => r.CreatedAt))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(d => d.Id == id, ct).ConfigureAwait(false);

    public async Task<PagedResult<Discussion>> GetByCourseIdAsync(int courseId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.CourseId == courseId);

        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<Discussion>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Discussion>> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Course)
            .Include(d => d.Replies);

        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<Discussion>(items, total, page, pageSize);
    }
}
