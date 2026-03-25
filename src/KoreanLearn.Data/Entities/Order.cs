using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>訂單實體，記錄使用者的課程購買訂單</summary>
public class Order : BaseEntity, ISoftDeletable
{
    /// <summary>下單使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>訂單編號（唯一）</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>訂單總金額</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>訂單狀態</summary>
    public OrderStatus Status { get; set; }

    /// <summary>付款狀態</summary>
    public PaymentStatus PaymentStatus { get; set; }

    /// <summary>付款完成時間</summary>
    public DateTime? PaidAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Order 的資料庫欄位與關聯設定</summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired().HasMaxLength(50);

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);
    }
}
