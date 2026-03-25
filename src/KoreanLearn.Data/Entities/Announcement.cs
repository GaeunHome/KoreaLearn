using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>公告實體，用於首頁或全站公告訊息</summary>
public class Announcement : BaseEntity, ISoftDeletable
{
    /// <summary>公告標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>公告內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>是否啟用顯示</summary>
    public bool IsActive { get; set; }

    /// <summary>公告開始日期</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>公告結束日期</summary>
    public DateTime? EndDate { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Announcement 的資料庫欄位與關聯設定</summary>
public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(a => a.Content)
            .IsRequired().HasMaxLength(4000);

        builder.HasIndex(a => a.IsActive);
    }
}
