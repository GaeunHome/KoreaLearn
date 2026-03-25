using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>測驗選項實體，代表選擇題中的一個選項</summary>
public class QuizOption : BaseEntity
{
    /// <summary>所屬題目 ID</summary>
    public int QuestionId { get; set; }

    /// <summary>選項內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>是否為正確答案</summary>
    public bool IsCorrect { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public QuizQuestion Question { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>QuizOption 的資料庫欄位與關聯設定</summary>
public class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
{
    public void Configure(EntityTypeBuilder<QuizOption> builder)
    {
        builder.ToTable("QuizOptions");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Content)
            .IsRequired().HasMaxLength(1000);

        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.QuestionId);

        builder.HasQueryFilter(o => !o.Question.Quiz.IsDeleted);
    }
}
