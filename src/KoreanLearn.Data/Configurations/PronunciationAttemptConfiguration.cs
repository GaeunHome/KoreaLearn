using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

public class PronunciationAttemptConfiguration : IEntityTypeConfiguration<PronunciationAttempt>
{
    public void Configure(EntityTypeBuilder<PronunciationAttempt> builder)
    {
        builder.ToTable("PronunciationAttempts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.RecordingUrl)
            .IsRequired().HasMaxLength(500);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Exercise)
            .WithMany(e => e.Attempts)
            .HasForeignKey(a => a.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.ExerciseId);
    }
}
