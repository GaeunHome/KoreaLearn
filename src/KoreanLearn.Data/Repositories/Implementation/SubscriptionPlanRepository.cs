using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class SubscriptionPlanRepository(ApplicationDbContext db)
    : Repository<SubscriptionPlan>(db), ISubscriptionPlanRepository
{
    public async Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansAsync(CancellationToken ct = default)
        => await DbSet.Where(p => p.IsActive).OrderBy(p => p.MonthlyPrice).ToListAsync(ct).ConfigureAwait(false);
}
