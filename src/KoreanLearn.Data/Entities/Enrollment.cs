using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

public class Enrollment : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
