using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>課程實體，代表一門韓語課程</summary>
public class Course : BaseEntity, ISoftDeletable
{
    /// <summary>課程標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>課程描述</summary>
    public string? Description { get; set; }

    /// <summary>封面圖片路徑</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>課程售價</summary>
    public decimal Price { get; set; }

    /// <summary>難度等級（初級/中級/高級）</summary>
    public DifficultyLevel Level { get; set; }

    /// <summary>是否已上架發佈</summary>
    public bool IsPublished { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    /// <summary>授課教師 ID（對應 AppUser）</summary>
    public string? TeacherId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser? Teacher { get; set; }
    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Discussion> Discussions { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Course 的資料庫欄位與關聯設定</summary>
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(4000);

        builder.Property(c => c.CoverImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Price)
            .HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(c => c.TeacherId).HasMaxLength(450);
        builder.HasOne(c => c.Teacher)
            .WithMany(u => u.TeacherCourses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.Title);
        builder.HasIndex(c => c.IsPublished);
        builder.HasIndex(c => c.TeacherId);
    }
}
