namespace KoreanLearn.Data.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int CourseId { get; set; }
    public decimal Price { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
