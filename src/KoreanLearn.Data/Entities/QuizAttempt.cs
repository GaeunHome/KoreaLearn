namespace KoreanLearn.Data.Entities;

public class QuizAttempt : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int QuizId { get; set; }
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public bool IsPassed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
    public ICollection<QuizAnswer> Answers { get; set; } = [];
}
