using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Admin.Quiz;

/// <summary>建立/編輯測驗表單 ViewModel</summary>
public class QuizFormViewModel
{
    /// <summary>測驗 ID（編輯時使用）</summary>
    public int Id { get; set; }

    /// <summary>所屬單元 ID</summary>
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

    /// <summary>所屬單元標題（顯示用）</summary>
    public string? LessonTitle { get; set; }

    /// <summary>所屬課程標題（顯示用）</summary>
    public string? CourseTitle { get; set; }

    /// <summary>所屬課程 ID（導覽用）</summary>
    public int? CourseId { get; set; }
}

/// <summary>後台測驗詳情 ViewModel（含題目與選項列表）</summary>
public class QuizDetailViewModel
{
    /// <summary>測驗 ID</summary>
    public int Id { get; set; }

    /// <summary>測驗標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>測驗說明</summary>
    public string? Description { get; set; }

    /// <summary>及格分數</summary>
    public int PassingScore { get; set; }

    /// <summary>時間限制（分鐘）</summary>
    public int TimeLimitMinutes { get; set; }

    /// <summary>所屬單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>所屬單元標題</summary>
    public string? LessonTitle { get; set; }

    /// <summary>所屬課程標題</summary>
    public string? CourseTitle { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int? CourseId { get; set; }

    /// <summary>題目列表</summary>
    public IReadOnlyList<QuestionViewModel> Questions { get; set; } = [];

    /// <summary>全部題目的總配分</summary>
    public int TotalPoints => Questions.Sum(q => q.Points);
}

/// <summary>題目 ViewModel（後台顯示用）</summary>
public class QuestionViewModel
{
    /// <summary>題目 ID</summary>
    public int Id { get; set; }

    /// <summary>題目內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>題型中文顯示</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>題型列舉值</summary>
    public int TypeValue { get; set; }

    /// <summary>配分</summary>
    public int Points { get; set; }

    /// <summary>排序權重</summary>
    public int SortOrder { get; set; }

    /// <summary>正確答案（填空題）</summary>
    public string? CorrectAnswer { get; set; }

    /// <summary>選項列表</summary>
    public IReadOnlyList<OptionViewModel> Options { get; set; } = [];
}

/// <summary>選項 ViewModel（後台顯示用）</summary>
public class OptionViewModel
{
    /// <summary>選項 ID</summary>
    public int Id { get; set; }

    /// <summary>選項內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>是否為正確答案</summary>
    public bool IsCorrect { get; set; }

    /// <summary>排序權重</summary>
    public int SortOrder { get; set; }
}
