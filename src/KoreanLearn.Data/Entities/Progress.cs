namespace KoreanLearn.Data.Entities;

public class Progress : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int VideoProgressSeconds { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
