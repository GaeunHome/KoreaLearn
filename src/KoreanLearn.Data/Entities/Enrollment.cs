using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

/// <summary>選課紀錄實體，記錄使用者與課程的關聯及學習進度</summary>
public class Enrollment : BaseEntity
{
    // ==================== 基本資訊 ====================
    public EnrollmentStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; }

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
