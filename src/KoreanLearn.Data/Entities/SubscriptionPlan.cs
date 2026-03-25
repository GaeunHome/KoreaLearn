using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>訂閱方案實體，定義可購買的訂閱計畫（月費、時長等）</summary>
public class SubscriptionPlan : BaseEntity, ISoftDeletable
{
    /// <summary>方案名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>方案描述</summary>
    public string? Description { get; set; }

    /// <summary>月費金額</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>訂閱時長（月）</summary>
    public int DurationMonths { get; set; }

    /// <summary>是否啟用上架</summary>
    public bool IsActive { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public ICollection<UserSubscription> Subscriptions { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>SubscriptionPlan 的資料庫欄位與關聯設定</summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired().HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.MonthlyPrice)
            .HasColumnType("decimal(18,2)").IsRequired();
    }
}
