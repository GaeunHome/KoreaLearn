using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Learn;

/// <summary>前台測驗作答頁面 ViewModel（題目與選項，不含正確答案）</summary>
public class QuizTakeViewModel
{
    /// <summary>測驗 ID</summary>
    public int QuizId { get; set; }

    /// <summary>測驗標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>測驗說明</summary>
    public string? Description { get; set; }

    /// <summary>及格分數</summary>
    public int PassingScore { get; set; }

    /// <summary>時間限制（分鐘）</summary>
    public int TimeLimitMinutes { get; set; }

    /// <summary>全部題目的總配分</summary>
    public int TotalPoints { get; set; }

    /// <summary>題目列表</summary>
    public IReadOnlyList<QuizQuestionItem> Questions { get; set; } = [];

    // ── 導覽資訊 ──

    /// <summary>所屬單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>所屬單元標題</summary>
    public string? LessonTitle { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>所屬課程標題</summary>
    public string? CourseTitle { get; set; }
}

/// <summary>作答頁面的題目項目</summary>
public class QuizQuestionItem
{
    /// <summary>題目 ID</summary>
    public int QuestionId { get; set; }

    /// <summary>題目內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>題型</summary>
    public QuestionType Type { get; set; }

    /// <summary>配分</summary>
    public int Points { get; set; }

    /// <summary>選項列表（選擇題用）</summary>
    public IReadOnlyList<QuizOptionItem> Options { get; set; } = [];
}

/// <summary>作答頁面的選項項目（不含正確答案標記）</summary>
public class QuizOptionItem
{
    /// <summary>選項 ID</summary>
    public int OptionId { get; set; }

    /// <summary>選項內容</summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>測驗結果 ViewModel（含各題答對與否）</summary>
public class QuizResultViewModel
{
    /// <summary>作答紀錄 ID</summary>
    public int AttemptId { get; set; }

    /// <summary>測驗標題</summary>
    public string QuizTitle { get; set; } = string.Empty;

    /// <summary>得分</summary>
    public int Score { get; set; }

    /// <summary>總配分</summary>
    public int TotalPoints { get; set; }

    /// <summary>及格分數</summary>
    public int PassingScore { get; set; }

    /// <summary>是否通過</summary>
    public bool IsPassed { get; set; }

    /// <summary>開始作答時間</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>完成作答時間</summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>各題作答結果</summary>
    public IReadOnlyList<QuizAnswerResult> Answers { get; set; } = [];

    /// <summary>得分百分比</summary>
    public int ScorePercent => TotalPoints > 0 ? Score * 100 / TotalPoints : 0;

    // ── 導覽資訊 ──

    /// <summary>測驗 ID</summary>
    public int QuizId { get; set; }

    /// <summary>所屬單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }
}

/// <summary>單題作答結果 ViewModel</summary>
public class QuizAnswerResult
{
    /// <summary>題目內容</summary>
    public string QuestionContent { get; set; } = string.Empty;

    /// <summary>題型</summary>
    public QuestionType QuestionType { get; set; }

    /// <summary>題目配分</summary>
    public int Points { get; set; }

    /// <summary>實際得分</summary>
    public int PointsEarned { get; set; }

    /// <summary>是否答對</summary>
    public bool IsCorrect { get; set; }

    /// <summary>使用者選擇的答案</summary>
    public string? SelectedAnswer { get; set; }

    /// <summary>正確答案</summary>
    public string? CorrectAnswer { get; set; }
}

/// <summary>測驗提交模型（使用者答案）</summary>
public class QuizSubmitModel
{
    /// <summary>測驗 ID</summary>
    public int QuizId { get; set; }

    /// <summary>答案字典（Key=題目ID, Value=使用者答案）</summary>
    public Dictionary<int, string> Answers { get; set; } = new();
}
