using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>討論區回覆實體，代表對討論主題的一則回覆</summary>
public class DiscussionReply : BaseEntity, ISoftDeletable
{
    /// <summary>回覆使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>所屬討論主題 ID</summary>
    public int DiscussionId { get; set; }

    /// <summary>回覆內容</summary>
    public string Content { get; set; } = string.Empty;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public Discussion Discussion { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>DiscussionReply 的資料庫欄位與關聯設定</summary>
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
