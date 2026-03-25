namespace KoreanLearn.Data.Entities;

public class PronunciationAttempt : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int ExerciseId { get; set; }
    public string RecordingUrl { get; set; } = string.Empty;

    // Navigation
    public AppUser User { get; set; } = null!;
    public PronunciationExercise Exercise { get; set; } = null!;
}
