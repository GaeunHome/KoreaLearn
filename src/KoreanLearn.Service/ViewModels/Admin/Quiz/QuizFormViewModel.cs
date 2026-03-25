using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Admin.Quiz;

public class QuizFormViewModel
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題須介於 1–200 字元")]
    [Display(Name = "測驗標題")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "說明")]
    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "及格分數為必填")]
    [Range(0, 100, ErrorMessage = "及格分數須在 0–100 之間")]
    [Display(Name = "及格分數")]
    public int PassingScore { get; set; } = 70;

    [Display(Name = "時間限制（分鐘）")]
    [Range(0, 180, ErrorMessage = "時間限制須在 0–180 之間")]
    public int TimeLimitMinutes { get; set; }

    // Display context
    public string? LessonTitle { get; set; }
    public string? CourseTitle { get; set; }
    public int? CourseId { get; set; }
}

public class QuizDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PassingScore { get; set; }
    public int TimeLimitMinutes { get; set; }
    public int LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public string? CourseTitle { get; set; }
    public int? CourseId { get; set; }
    public IReadOnlyList<QuestionViewModel> Questions { get; set; } = [];
    public int TotalPoints => Questions.Sum(q => q.Points);
}

public class QuestionViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int TypeValue { get; set; }
    public int Points { get; set; }
    public int SortOrder { get; set; }
    public string? CorrectAnswer { get; set; }
    public IReadOnlyList<OptionViewModel> Options { get; set; } = [];
}

public class OptionViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}
