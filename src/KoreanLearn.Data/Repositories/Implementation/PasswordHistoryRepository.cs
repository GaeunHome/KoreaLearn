using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

/// <summary>密碼歷史記錄 Repository 實作</summary>
public class PasswordHistoryRepository(ApplicationDbContext db) : IPasswordHistoryRepository
{
    private readonly DbSet<PasswordHistory> _dbSet = db.Set<PasswordHistory>();

    /// <inheritdoc />
    public async Task<IReadOnlyList<PasswordHistory>> GetByUserIdAsync(string userId, int count = 5, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task AddAsync(PasswordHistory entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct).ConfigureAwait(false);
}
