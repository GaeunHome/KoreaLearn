using KoreanLearn.Data.Entities;

namespace KoreanLearn.Data.Repositories.Interfaces;

/// <summary>使用者訂閱 Repository 介面，提供訂閱狀態的查詢</summary>
public interface IUserSubscriptionRepository : IRepository<UserSubscription>
{
    /// <summary>取得使用者目前有效的訂閱紀錄</summary>
    Task<UserSubscription?> GetActiveByUserAsync(string userId, CancellationToken ct = default);
}
