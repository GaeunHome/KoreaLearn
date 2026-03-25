namespace KoreanLearn.Data.Entities;

public class UserSubscription : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}
