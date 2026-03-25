using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

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
