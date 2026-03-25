using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>
/// 字卡學習紀錄實體，使用 SM-2 間隔重複演算法追蹤每張字卡的複習排程。
/// Quality（0-5）為使用者自評的記憶品質，EaseFactor 控制間隔增長速度，
/// Interval 為下次複習的間隔天數，Repetition 為連續正確次數。
/// </summary>
public class FlashcardLog : BaseEntity
{
    /// <summary>使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>字卡 ID</summary>
    public int FlashcardId { get; set; }

    /// <summary>SM-2 品質評分（0=完全忘記，5=完美記憶）</summary>
    public int Quality { get; set; }

    /// <summary>SM-2 難易度因子，影響間隔增長速度（初始值 2.5）</summary>
    public double EaseFactor { get; set; } = 2.5;

    /// <summary>SM-2 複習間隔天數</summary>
    public int Interval { get; set; } = 1;

    /// <summary>SM-2 連續正確作答次數</summary>
    public int Repetition { get; set; }

    /// <summary>下次複習日期</summary>
    public DateTime NextReviewDate { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public Flashcard Flashcard { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>FlashcardLog 的資料庫欄位與關聯設定</summary>
public class FlashcardLogConfiguration : IEntityTypeConfiguration<FlashcardLog>
{
    public void Configure(EntityTypeBuilder<FlashcardLog> builder)
    {
        builder.ToTable("FlashcardLogs");
        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.User)
            .WithMany(u => u.FlashcardLogs)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Flashcard)
            .WithMany(f => f.Logs)
            .HasForeignKey(l => l.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.UserId, l.FlashcardId });
        builder.HasIndex(l => l.NextReviewDate);

        builder.HasQueryFilter(l => !l.Flashcard.Deck.IsDeleted);
    }
}
