using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Learn;

public class QuizTakeViewModel
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PassingScore { get; set; }
    public int TimeLimitMinutes { get; set; }
    public int TotalPoints { get; set; }
    public IReadOnlyList<QuizQuestionItem> Questions { get; set; } = [];

    // Context
    public int LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public int CourseId { get; set; }
    public string? CourseTitle { get; set; }
}

public class QuizQuestionItem
{
    public int QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Points { get; set; }
    public IReadOnlyList<QuizOptionItem> Options { get; set; } = [];
}

public class QuizOptionItem
{
    public int OptionId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class QuizResultViewModel
{
    public int AttemptId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public int PassingScore { get; set; }
    public bool IsPassed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public IReadOnlyList<QuizAnswerResult> Answers { get; set; } = [];

    public int ScorePercent => TotalPoints > 0 ? Score * 100 / TotalPoints : 0;

    // Context
    public int QuizId { get; set; }
    public int LessonId { get; set; }
    public int CourseId { get; set; }
}

public class QuizAnswerResult
{
    public string QuestionContent { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public int Points { get; set; }
    public int PointsEarned { get; set; }
    public bool IsCorrect { get; set; }
    public string? SelectedAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
}

public class QuizSubmitModel
{
    public int QuizId { get; set; }
    public Dictionary<int, string> Answers { get; set; } = new();
}
