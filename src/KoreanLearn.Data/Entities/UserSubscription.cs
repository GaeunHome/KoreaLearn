using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>使用者訂閱紀錄實體，記錄使用者目前的訂閱狀態與有效期間</summary>
public class UserSubscription : BaseEntity
{
    /// <summary>訂閱使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>訂閱方案 ID</summary>
    public int PlanId { get; set; }

    /// <summary>訂閱開始日期</summary>
    public DateTime StartDate { get; set; }

    /// <summary>訂閱到期日期</summary>
    public DateTime EndDate { get; set; }

    /// <summary>是否為目前有效的訂閱</summary>
    public bool IsActive { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>UserSubscription 的資料庫欄位與關聯設定</summary>
public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscriptions");
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.User)
            .WithOne(u => u.ActiveSubscription)
            .HasForeignKey<UserSubscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.UserId);

        builder.HasQueryFilter(s => !s.Plan.IsDeleted);
    }
}
