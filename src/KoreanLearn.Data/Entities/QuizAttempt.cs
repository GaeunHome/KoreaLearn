using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>測驗作答紀錄實體，記錄學生的一次測驗嘗試與成績</summary>
public class QuizAttempt : BaseEntity
{
    /// <summary>作答使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>測驗 ID</summary>
    public int QuizId { get; set; }

    /// <summary>獲得的總分</summary>
    public int Score { get; set; }

    /// <summary>測驗滿分</summary>
    public int TotalPoints { get; set; }

    /// <summary>是否及格</summary>
    public bool IsPassed { get; set; }

    /// <summary>開始作答時間</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>完成作答時間</summary>
    public DateTime? FinishedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
    public ICollection<QuizAnswer> Answers { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>QuizAttempt 的資料庫欄位與關聯設定</summary>
public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempts");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.User)
            .WithMany(u => u.QuizAttempts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Quiz)
            .WithMany(q => q.Attempts)
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.QuizId);

        builder.HasQueryFilter(a => !a.Quiz.IsDeleted);
    }
}
