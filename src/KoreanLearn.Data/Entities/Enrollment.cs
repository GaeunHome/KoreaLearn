using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>選課紀錄實體，記錄使用者與課程的關聯及學習進度</summary>
public class Enrollment : BaseEntity
{
    /// <summary>使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>選課狀態（進行中/已完成等）</summary>
    public EnrollmentStatus Status { get; set; }

    /// <summary>完課時間</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>學習進度百分比（0-100）</summary>
    public int ProgressPercent { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Enrollment 的資料庫欄位與關聯設定</summary>
public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        builder.HasIndex(e => e.UserId);

        builder.HasQueryFilter(e => !e.Course.IsDeleted);
    }
}
