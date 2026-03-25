using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

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
