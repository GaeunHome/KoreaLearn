using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>測驗題目實體，支援選擇題與填空題兩種題型</summary>
public class QuizQuestion : BaseEntity
{
    /// <summary>所屬測驗 ID</summary>
    public int QuizId { get; set; }

    /// <summary>題目內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>題目類型（選擇題/填空題）</summary>
    public QuestionType Type { get; set; }

    /// <summary>配分</summary>
    public int Points { get; set; } = 1;

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    /// <summary>填空題的正確答案（僅 QuestionType.FillInBlank 使用）</summary>
    public string? CorrectAnswer { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Quiz Quiz { get; set; } = null!;
    public ICollection<QuizOption> Options { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>QuizQuestion 的資料庫欄位與關聯設定</summary>
public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Content)
            .IsRequired().HasMaxLength(2000);

        builder.Property(q => q.CorrectAnswer)
            .HasMaxLength(500);

        builder.HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.QuizId);

        builder.HasQueryFilter(q => !q.Quiz.IsDeleted);
    }
}
