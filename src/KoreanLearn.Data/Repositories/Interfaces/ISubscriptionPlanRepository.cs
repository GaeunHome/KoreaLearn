using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>訂閱方案 Repository 介面，提供方案資料的查詢</summary>
public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    /// <summary>取得所有啟用中的訂閱方案</summary>
    Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansAsync(CancellationToken ct = default);
}
