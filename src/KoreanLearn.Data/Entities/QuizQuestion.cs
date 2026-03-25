using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

public class QuizQuestion : BaseEntity
{
    public int QuizId { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Points { get; set; } = 1;
    public int SortOrder { get; set; }
    public string? CorrectAnswer { get; set; } // FillInBlank 的正確答案

    // Navigation
    public Quiz Quiz { get; set; } = null!;
    public ICollection<QuizOption> Options { get; set; } = [];
}
