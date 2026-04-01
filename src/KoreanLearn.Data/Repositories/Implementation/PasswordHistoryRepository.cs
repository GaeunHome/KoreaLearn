using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

/// <summary>密碼歷史記錄 Repository 實作</summary>
public class PasswordHistoryRepository(ApplicationDbContext db) : Repository<PasswordHistory>(db), IPasswordHistoryRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PasswordHistory>> GetByUserIdAsync(string userId, int count = 5, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(ct).ConfigureAwait(false);
}
