using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class AnnouncementRepository(ApplicationDbContext db) : Repository<Announcement>(db), IAnnouncementRepository
{
    public async Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(a => a.IsActive &&
                (a.StartDate == null || a.StartDate <= DateTime.UtcNow) &&
                (a.EndDate == null || a.EndDate >= DateTime.UtcNow))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct).ConfigureAwait(false);
}
