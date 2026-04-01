using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

/// <summary>系統參數 Repository 實作</summary>
public class SystemSettingRepository(ApplicationDbContext db) : Repository<SystemSetting>(db), ISystemSettingRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SystemSetting>> GetAllOrderedAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .OrderBy(s => s.Group).ThenBy(s => s.Key)
            .ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(s => s.Key == key, ct).ConfigureAwait(false);
}
