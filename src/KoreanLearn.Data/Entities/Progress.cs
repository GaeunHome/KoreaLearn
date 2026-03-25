using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>學習進度實體，記錄使用者對每個單元的完成狀態與影片播放進度</summary>
public class Progress : BaseEntity
{
    /// <summary>使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>是否已完成該單元</summary>
    public bool IsCompleted { get; set; }

    /// <summary>完成時間</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>影片播放進度（秒），用於續播功能</summary>
    public int VideoProgressSeconds { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Progress 的資料庫欄位與關聯設定</summary>
public class ProgressConfiguration : IEntityTypeConfiguration<Progress>
{
    public void Configure(EntityTypeBuilder<Progress> builder)
    {
        builder.ToTable("Progresses");
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Lesson)
            .WithMany(l => l.Progresses)
            .HasForeignKey(p => p.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.UserId, p.LessonId }).IsUnique();
        builder.HasIndex(p => p.UserId);

        builder.HasQueryFilter(p => !p.Lesson.IsDeleted);
    }
}
