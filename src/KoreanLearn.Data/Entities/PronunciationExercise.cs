namespace KoreanLearn.Data.Entities;

public class PronunciationExercise : BaseEntity, ISoftDeletable
{
    public string Korean { get; set; } = string.Empty;
    public string? Romanization { get; set; }
    public string? Chinese { get; set; }
    public string StandardAudioUrl { get; set; } = string.Empty;
    public int? LessonId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Lesson? Lesson { get; set; }
    public ICollection<PronunciationAttempt> Attempts { get; set; } = [];
}
