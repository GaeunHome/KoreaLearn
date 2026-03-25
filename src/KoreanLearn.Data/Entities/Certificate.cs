using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

public class Certificate : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public string? PdfUrl { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

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
