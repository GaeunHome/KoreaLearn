using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>
/// 單元實體，代表章節下的一個學習單元。
/// 根據 Type 欄位決定使用哪組條件欄位：Video（VideoUrl/VideoDurationSeconds）、Article（ArticleContent）、Pdf（PdfUrl/PdfFileName）
/// </summary>
public class Lesson : BaseEntity, ISoftDeletable
{
    /// <summary>單元標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>單元描述</summary>
    public string? Description { get; set; }

    /// <summary>單元類型（影片/文章/PDF）</summary>
    public LessonType Type { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    /// <summary>所屬章節 ID</summary>
    public int SectionId { get; set; }

    /// <summary>是否為免費預覽單元</summary>
    public bool IsFreePreview { get; set; }

    // ── 條件欄位：根據 Type 只有對應的欄位會有值 ────────
    /// <summary>影片網址（僅 LessonType.Video）</summary>
    public string? VideoUrl { get; set; }

    /// <summary>影片時長（秒）（僅 LessonType.Video）</summary>
    public int? VideoDurationSeconds { get; set; }

    /// <summary>文章內容 HTML（僅 LessonType.Article）</summary>
    public string? ArticleContent { get; set; }

    /// <summary>PDF 檔案路徑（僅 LessonType.Pdf）</summary>
    public string? PdfUrl { get; set; }

    /// <summary>PDF 原始檔名（僅 LessonType.Pdf）</summary>
    public string? PdfFileName { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Section Section { get; set; } = null!;
    public ICollection<Progress> Progresses { get; set; } = [];
    public ICollection<LessonAttachment> Attachments { get; set; } = [];
    public Quiz? Quiz { get; set; }
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Lesson 的資料庫欄位與關聯設定</summary>
public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(2000);

        builder.Property(l => l.VideoUrl)
            .HasMaxLength(500);

        builder.Property(l => l.PdfUrl)
            .HasMaxLength(500);

        builder.Property(l => l.PdfFileName)
            .HasMaxLength(200);

        builder.HasOne(l => l.Section)
            .WithMany(s => s.Lessons)
            .HasForeignKey(l => l.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.SectionId);
    }
}
