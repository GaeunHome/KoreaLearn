using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansAsync(CancellationToken ct = default);
}

public interface IUserSubscriptionRepository : IRepository<UserSubscription>
{
    Task<UserSubscription?> GetActiveByUserAsync(string userId, CancellationToken ct = default);
}
