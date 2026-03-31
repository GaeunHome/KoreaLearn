using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

/// <summary>幻燈片橫幅 Repository 實作</summary>
public class BannerRepository(ApplicationDbContext db) : Repository<Banner>(db), IBannerRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Banner>> GetActiveOrderedAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(b => b.Course)
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(ct).ConfigureAwait(false);
}
