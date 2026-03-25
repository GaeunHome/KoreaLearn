using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>訂單明細實體，記錄訂單中每一門課程的購買價格</summary>
public class OrderItem : BaseEntity
{
    /// <summary>所屬訂單 ID</summary>
    public int OrderId { get; set; }

    /// <summary>購買的課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>購買時的課程價格</summary>
    public decimal Price { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Order Order { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>OrderItem 的資料庫欄位與關聯設定</summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Price)
            .HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Course)
            .WithMany()
            .HasForeignKey(oi => oi.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(oi => oi.OrderId);

        builder.HasQueryFilter(oi => !oi.Course.IsDeleted);
    }
}
