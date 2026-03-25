namespace KoreanLearn.Data.Entities;

public class Quiz : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int LessonId { get; set; }
    public int PassingScore { get; set; } = 70;
    public int TimeLimitMinutes { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Lesson Lesson { get; set; } = null!;
    public ICollection<QuizQuestion> Questions { get; set; } = [];
    public ICollection<QuizAttempt> Attempts { get; set; } = [];
}
