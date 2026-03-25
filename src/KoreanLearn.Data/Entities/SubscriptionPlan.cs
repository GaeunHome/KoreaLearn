namespace KoreanLearn.Data.Entities;

public class SubscriptionPlan : BaseEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int DurationMonths { get; set; }
    public bool IsActive { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<UserSubscription> Subscriptions { get; set; } = [];
}
