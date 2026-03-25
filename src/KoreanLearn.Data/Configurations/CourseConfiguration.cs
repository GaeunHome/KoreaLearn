using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

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

        builder.HasIndex(c => c.Title);
        builder.HasIndex(c => c.IsPublished);
    }
}
