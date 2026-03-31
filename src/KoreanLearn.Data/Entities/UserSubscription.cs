namespace KoreanLearn.Data.Entities;

/// <summary>使用者訂閱紀錄實體，記錄使用者目前的訂閱狀態與有效期間</summary>
public class UserSubscription : BaseEntity
{
    // ==================== 基本資訊 ====================
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int PlanId { get; set; }
    public SubscriptionPlan Plan { get; set; } = null!;
}
