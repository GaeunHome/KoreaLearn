namespace KoreanLearn.Data.Entities;

/// <summary>測驗作答紀錄實體，記錄學生每一題的作答內容與得分</summary>
public class QuizAnswer : BaseEntity
{
    // ==================== 基本資訊 ====================
    /// <summary>選擇題選中的選項 ID（填空題為 null）</summary>
    public int? SelectedOptionId { get; set; }

    /// <summary>填空題的文字作答內容（選擇題為 null）</summary>
    public string? TextAnswer { get; set; }

    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }

    // ==================== 關聯 ====================
    public int AttemptId { get; set; }
    public QuizAttempt Attempt { get; set; } = null!;

    public int QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;

    public QuizOption? SelectedOption { get; set; }
}
