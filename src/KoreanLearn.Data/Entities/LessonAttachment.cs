using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>單元附件實體，儲存單元的補充教材檔案</summary>
public class LessonAttachment : BaseEntity, ISoftDeletable
{
    /// <summary>所屬單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>檔案名稱</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>檔案儲存路徑</summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>檔案大小（位元組）</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Lesson Lesson { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>LessonAttachment 的資料庫欄位與關聯設定</summary>
public class LessonAttachmentConfiguration : IEntityTypeConfiguration<LessonAttachment>
{
    public void Configure(EntityTypeBuilder<LessonAttachment> builder)
    {
        builder.ToTable("LessonAttachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(500);
        builder.Property(a => a.FileUrl).IsRequired().HasMaxLength(1000);

        builder.HasOne(a => a.Lesson)
            .WithMany(l => l.Attachments)
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.LessonId);
    }
}
