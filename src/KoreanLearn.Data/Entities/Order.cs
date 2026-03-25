using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

public class Order : BaseEntity, ISoftDeletable
{
    public string UserId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
