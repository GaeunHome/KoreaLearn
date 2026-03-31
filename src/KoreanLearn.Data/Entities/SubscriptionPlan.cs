namespace KoreanLearn.Data.Entities;

/// <summary>訂閱方案實體，定義可購買的訂閱計畫</summary>
public class SubscriptionPlan : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal MonthlyPrice { get; set; }

    public int DurationMonths { get; set; }
    public bool IsActive { get; set; }

    // ==================== 關聯 ====================
    public ICollection<UserSubscription> Subscriptions { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
