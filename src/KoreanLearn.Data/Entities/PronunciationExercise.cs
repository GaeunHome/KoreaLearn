using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>發音練習實體，包含標準韓文發音音檔供學生跟讀練習</summary>
public class PronunciationExercise : BaseEntity, ISoftDeletable
{
    /// <summary>韓文文字</summary>
    public string Korean { get; set; } = string.Empty;

    /// <summary>羅馬拼音</summary>
    public string? Romanization { get; set; }

    /// <summary>中文翻譯</summary>
    public string? Chinese { get; set; }

    /// <summary>標準發音音檔路徑</summary>
    public string StandardAudioUrl { get; set; } = string.Empty;

    /// <summary>關聯單元 ID（可選）</summary>
    public int? LessonId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Lesson? Lesson { get; set; }
    public ICollection<PronunciationAttempt> Attempts { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>PronunciationExercise 的資料庫欄位與關聯設定</summary>
public class PronunciationExerciseConfiguration : IEntityTypeConfiguration<PronunciationExercise>
{
    public void Configure(EntityTypeBuilder<PronunciationExercise> builder)
    {
        builder.ToTable("PronunciationExercises");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Korean)
            .IsRequired().HasMaxLength(200);

        builder.Property(p => p.Romanization)
            .HasMaxLength(200);

        builder.Property(p => p.Chinese)
            .HasMaxLength(200);

        builder.Property(p => p.StandardAudioUrl)
            .IsRequired().HasMaxLength(500);

        builder.HasOne(p => p.Lesson)
            .WithMany()
            .HasForeignKey(p => p.LessonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
