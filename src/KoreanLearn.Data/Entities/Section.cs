using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>章節實體，代表課程下的一個章節</summary>
public class Section : BaseEntity, ISoftDeletable
{
    /// <summary>章節標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>章節描述</summary>
    public string? Description { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Section 的資料庫欄位與關聯設定</summary>
public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.HasOne(s => s.Course)
            .WithMany(c => c.Sections)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.CourseId);
    }
}
