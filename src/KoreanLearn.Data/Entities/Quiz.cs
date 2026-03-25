using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>測驗實體，代表一個單元對應的測驗</summary>
public class Quiz : BaseEntity, ISoftDeletable
{
    /// <summary>測驗標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>測驗說明</summary>
    public string? Description { get; set; }

    /// <summary>所屬單元 ID（一對一關係）</summary>
    public int LessonId { get; set; }

    /// <summary>及格分數（預設 70）</summary>
    public int PassingScore { get; set; } = 70;

    /// <summary>作答時間限制（分鐘），0 表示不限時</summary>
    public int TimeLimitMinutes { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Lesson Lesson { get; set; } = null!;
    public ICollection<QuizQuestion> Questions { get; set; } = [];
    public ICollection<QuizAttempt> Attempts { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Quiz 的資料庫欄位與關聯設定</summary>
public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(q => q.Description)
            .HasMaxLength(2000);

        builder.HasOne(q => q.Lesson)
            .WithOne(l => l.Quiz)
            .HasForeignKey<Quiz>(q => q.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => q.LessonId).IsUnique();
    }
}
