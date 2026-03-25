using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>討論區主題實體，代表課程討論區中的一則討論</summary>
public class Discussion : BaseEntity, ISoftDeletable
{
    /// <summary>發文使用者 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>討論標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>討論內容</summary>
    public string Content { get; set; } = string.Empty;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<DiscussionReply> Replies { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Discussion 的資料庫欄位與關聯設定</summary>
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
