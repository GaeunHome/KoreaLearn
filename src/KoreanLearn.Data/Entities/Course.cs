using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

public class Course : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public DifficultyLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
    public string? TeacherId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public AppUser? Teacher { get; set; }
    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Discussion> Discussions { get; set; } = [];
}

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
