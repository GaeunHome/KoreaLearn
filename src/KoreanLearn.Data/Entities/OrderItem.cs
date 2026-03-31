namespace KoreanLearn.Data.Entities;

/// <summary>訂單明細實體，記錄訂單中每一門課程的購買價格</summary>
public class OrderItem : BaseEntity
{
    // ==================== 基本資訊 ====================
    public decimal Price { get; set; }

    // ==================== 關聯 ====================
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
