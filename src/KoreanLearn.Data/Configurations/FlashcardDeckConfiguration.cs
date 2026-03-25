using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

public class FlashcardDeckConfiguration : IEntityTypeConfiguration<FlashcardDeck>
{
    public void Configure(EntityTypeBuilder<FlashcardDeck> builder)
    {
        builder.ToTable("FlashcardDecks");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(2000);

        builder.HasOne(d => d.Course)
            .WithMany()
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
