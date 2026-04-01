using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using KoreanLearn.Library.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

/// <summary>公告 Repository 實作</summary>
public class AnnouncementRepository(ApplicationDbContext db) : Repository<Announcement>(db), IAnnouncementRepository
{
    private IQueryable<Announcement> ActiveQuery()
        => DbSet.AsNoTracking()
            .Where(a => a.IsActive &&
                (a.StartDate == null || a.StartDate <= DateTime.UtcNow) &&
                (a.EndDate == null || a.EndDate >= DateTime.UtcNow));

    private static IOrderedQueryable<Announcement> ApplyDefaultSort(IQueryable<Announcement> query)
        => query.OrderByDescending(a => a.IsPinned)
            .ThenBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt);

    public async Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default)
        => await ApplyDefaultSort(ActiveQuery())
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Announcement>> GetLatestActiveAsync(int count, CancellationToken ct = default)
        => await ApplyDefaultSort(ActiveQuery())
            .Take(count)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<PagedResult<Announcement>> GetPublishedPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = ApplyDefaultSort(ActiveQuery().Include(a => a.Attachments));
        var total = await ActiveQuery().CountAsync(ct).ConfigureAwait(false);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);
        return new PagedResult<Announcement>(items, total, page, pageSize);
    }

    public async Task<Announcement?> GetPublishedWithAttachmentsAsync(int id, CancellationToken ct = default)
        => await ActiveQuery()
            .Include(a => a.Attachments.OrderBy(at => at.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Announcement>> GetAllIncludeDeletedAsync(CancellationToken ct = default)
        => await DbSet.IgnoreQueryFilters().AsNoTracking()
            .Include(a => a.Attachments)
            .OrderByDescending(a => a.IsPinned)
            .ThenBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<Announcement?> GetWithAttachmentsIncludeDeletedAsync(int id, CancellationToken ct = default)
        => await DbSet.IgnoreQueryFilters()
            .Include(a => a.Attachments.OrderBy(at => at.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == id, ct).ConfigureAwait(false);

    public async Task<int> GetMaxSortOrderAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Select(a => (int?)a.SortOrder)
            .MaxAsync(ct).ConfigureAwait(false) ?? 0;
}
