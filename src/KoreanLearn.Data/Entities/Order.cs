using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

/// <summary>訂單實體，記錄使用者的課程購買訂單</summary>
public class Order : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
