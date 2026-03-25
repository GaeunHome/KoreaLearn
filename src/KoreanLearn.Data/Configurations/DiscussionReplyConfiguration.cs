using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

public class DiscussionReplyConfiguration : IEntityTypeConfiguration<DiscussionReply>
{
    public void Configure(EntityTypeBuilder<DiscussionReply> builder)
    {
        builder.ToTable("DiscussionReplies");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Content)
            .IsRequired().HasMaxLength(4000);

        builder.HasOne(r => r.User)
            .WithMany(u => u.DiscussionReplies)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Discussion)
            .WithMany(d => d.Replies)
            .HasForeignKey(r => r.DiscussionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.DiscussionId);
    }
}
