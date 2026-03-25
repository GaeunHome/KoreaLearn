namespace KoreanLearn.Data.Entities;

public class QuizAnswer : BaseEntity
{
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
    public string? TextAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }

    // Navigation
    public QuizAttempt Attempt { get; set; } = null!;
    public QuizQuestion Question { get; set; } = null!;
    public QuizOption? SelectedOption { get; set; }
}
