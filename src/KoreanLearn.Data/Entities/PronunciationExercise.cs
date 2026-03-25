using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

public class PronunciationExercise : BaseEntity, ISoftDeletable
{
    public string Korean { get; set; } = string.Empty;
    public string? Romanization { get; set; }
    public string? Chinese { get; set; }
    public string StandardAudioUrl { get; set; } = string.Empty;
    public int? LessonId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Lesson? Lesson { get; set; }
    public ICollection<PronunciationAttempt> Attempts { get; set; } = [];
}

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
