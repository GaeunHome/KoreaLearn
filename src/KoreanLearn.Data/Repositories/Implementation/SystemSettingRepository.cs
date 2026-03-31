using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

/// <summary>系統參數 Repository 實作</summary>
public class SystemSettingRepository(ApplicationDbContext db) : ISystemSettingRepository
{
    private readonly DbSet<SystemSetting> _dbSet = db.Set<SystemSetting>();

    /// <inheritdoc />
    public async Task<IReadOnlyList<SystemSetting>> GetAllOrderedAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .OrderBy(s => s.Group).ThenBy(s => s.Key)
            .ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(s => s.Key == key, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<SystemSetting?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task AddAsync(SystemSetting entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public void Update(SystemSetting entity) => _dbSet.Update(entity);

    /// <inheritdoc />
    public void Remove(SystemSetting entity) => _dbSet.Remove(entity);
}
