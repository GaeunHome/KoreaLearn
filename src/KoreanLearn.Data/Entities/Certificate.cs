using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>完課證書實體，記錄學生完成課程後獲頒的證書</summary>
public class Certificate : BaseEntity
{
    /// <summary>持有人 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>完成的課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>證書編號（唯一）</summary>
    public string CertificateNumber { get; set; } = string.Empty;

    /// <summary>頒發日期</summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>證書 PDF 檔案路徑</summary>
    public string? PdfUrl { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public AppUser User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Certificate 的資料庫欄位與關聯設定</summary>
public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CertificateNumber)
            .IsRequired().HasMaxLength(50);

        builder.Property(c => c.PdfUrl)
            .HasMaxLength(500);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.HasIndex(c => new { c.UserId, c.CourseId }).IsUnique();

        builder.HasQueryFilter(c => !c.Course.IsDeleted);
    }
}
