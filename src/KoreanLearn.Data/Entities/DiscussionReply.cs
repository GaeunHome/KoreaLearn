using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

public class DiscussionReply : BaseEntity, ISoftDeletable
{
    public string UserId { get; set; } = string.Empty;
    public int DiscussionId { get; set; }
    public string Content { get; set; } = string.Empty;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Discussion Discussion { get; set; } = null!;
}

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
