using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

public class FlashcardLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int FlashcardId { get; set; }
    public int Quality { get; set; }        // SM-2: 0-5 品質評分
    public double EaseFactor { get; set; } = 2.5;
    public int Interval { get; set; } = 1;  // 天數
    public int Repetition { get; set; }
    public DateTime NextReviewDate { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Flashcard Flashcard { get; set; } = null!;
}

public class FlashcardLogConfiguration : IEntityTypeConfiguration<FlashcardLog>
{
    public void Configure(EntityTypeBuilder<FlashcardLog> builder)
    {
        builder.ToTable("FlashcardLogs");
        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.User)
            .WithMany(u => u.FlashcardLogs)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Flashcard)
            .WithMany(f => f.Logs)
            .HasForeignKey(l => l.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.UserId, l.FlashcardId });
        builder.HasIndex(l => l.NextReviewDate);

        builder.HasQueryFilter(l => !l.Flashcard.Deck.IsDeleted);
    }
}
