using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>發音練習嘗試紀錄實體，記錄學生上傳的錄音檔</summary>
public class PronunciationAttempt : BaseEntity
{
    /// <summary>作答使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>對應發音練習 ID</summary>
    public int ExerciseId { get; set; }

    /// <summary>學生錄音檔案路徑</summary>
    public string RecordingUrl { get; set; } = string.Empty;

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public PronunciationExercise Exercise { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>PronunciationAttempt 的資料庫欄位與關聯設定</summary>
public class PronunciationAttemptConfiguration : IEntityTypeConfiguration<PronunciationAttempt>
{
    public void Configure(EntityTypeBuilder<PronunciationAttempt> builder)
    {
        builder.ToTable("PronunciationAttempts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.RecordingUrl)
            .IsRequired().HasMaxLength(500);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Exercise)
            .WithMany(e => e.Attempts)
            .HasForeignKey(a => a.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.ExerciseId);

        builder.HasQueryFilter(a => !a.Exercise.IsDeleted);
    }
}
