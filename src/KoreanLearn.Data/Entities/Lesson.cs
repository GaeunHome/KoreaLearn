using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

public class Lesson : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public int SortOrder { get; set; }
    public int SectionId { get; set; }
    public bool IsFreePreview { get; set; }

    // 根據 Type 只有一個會有值
    public string? VideoUrl { get; set; }           // LessonType.Video
    public int? VideoDurationSeconds { get; set; }   // LessonType.Video
    public string? ArticleContent { get; set; }      // LessonType.Article
    public string? PdfUrl { get; set; }              // LessonType.Pdf
    public string? PdfFileName { get; set; }         // LessonType.Pdf

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Section Section { get; set; } = null!;
    public ICollection<Progress> Progresses { get; set; } = [];
    public ICollection<LessonAttachment> Attachments { get; set; } = [];
    public Quiz? Quiz { get; set; }
}

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
