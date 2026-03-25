using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

public class DiscussionConfiguration : IEntityTypeConfiguration<Discussion>
{
    public void Configure(EntityTypeBuilder<Discussion> builder)
    {
        builder.ToTable("Discussions");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(d => d.Content)
            .IsRequired().HasMaxLength(4000);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Discussions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Course)
            .WithMany(c => c.Discussions)
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.CourseId);
    }
}
