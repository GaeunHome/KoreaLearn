namespace KoreanLearn.Data.Entities;

public class QuizOption : BaseEntity
{
    public int QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public QuizQuestion Question { get; set; } = null!;
}
