namespace KoreanLearn.Data.Entities;

/// <summary>完課證書實體，記錄學生完成課程後獲頒的證書</summary>
public class Certificate : BaseEntity
{
    // ==================== 基本資訊 ====================
    public string CertificateNumber { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; }

    public string? PdfUrl { get; set; }

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
