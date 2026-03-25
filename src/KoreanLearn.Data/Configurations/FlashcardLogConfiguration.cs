using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

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
