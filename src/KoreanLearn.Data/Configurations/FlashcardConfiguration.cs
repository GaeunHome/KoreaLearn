using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.ToTable("Flashcards");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Korean)
            .IsRequired().HasMaxLength(200);

        builder.Property(f => f.Chinese)
            .IsRequired().HasMaxLength(200);

        builder.Property(f => f.Romanization)
            .HasMaxLength(200);

        builder.Property(f => f.ExampleSentence)
            .HasMaxLength(1000);

        builder.Property(f => f.AudioUrl)
            .HasMaxLength(500);

        builder.HasOne(f => f.Deck)
            .WithMany(d => d.Flashcards)
            .HasForeignKey(f => f.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.DeckId);

        builder.HasQueryFilter(f => !f.Deck.IsDeleted);
    }
}
