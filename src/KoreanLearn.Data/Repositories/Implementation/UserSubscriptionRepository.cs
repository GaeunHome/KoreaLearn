using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class UserSubscriptionRepository(ApplicationDbContext db)
    : Repository<UserSubscription>(db), IUserSubscriptionRepository
{
    public async Task<UserSubscription?> GetActiveByUserAsync(string userId, CancellationToken ct = default)
        => await DbSet
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow, ct)
            .ConfigureAwait(false);
}
