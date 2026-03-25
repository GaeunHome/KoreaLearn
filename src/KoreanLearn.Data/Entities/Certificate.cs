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
