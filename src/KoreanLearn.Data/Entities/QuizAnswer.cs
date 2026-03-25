using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>測驗作答紀錄實體，記錄學生每一題的作答內容與得分</summary>
public class QuizAnswer : BaseEntity
{
    /// <summary>所屬作答紀錄 ID</summary>
    public int AttemptId { get; set; }

    /// <summary>對應題目 ID</summary>
    public int QuestionId { get; set; }

    /// <summary>選擇題選中的選項 ID（填空題為 null）</summary>
    public int? SelectedOptionId { get; set; }

    /// <summary>填空題的文字作答內容（選擇題為 null）</summary>
    public string? TextAnswer { get; set; }

    /// <summary>此題是否答對</summary>
    public bool IsCorrect { get; set; }

    /// <summary>此題獲得的分數</summary>
    public int PointsEarned { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public QuizAttempt Attempt { get; set; } = null!;
    public QuizQuestion Question { get; set; } = null!;
    public QuizOption? SelectedOption { get; set; }
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>QuizAnswer 的資料庫欄位與關聯設定</summary>
public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("QuizAnswers");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TextAnswer)
            .HasMaxLength(1000);

        builder.HasOne(a => a.Attempt)
            .WithMany(at => at.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.SelectedOption)
            .WithMany()
            .HasForeignKey(a => a.SelectedOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.AttemptId);

        builder.HasQueryFilter(a => !a.Attempt.Quiz.IsDeleted);
    }
}
