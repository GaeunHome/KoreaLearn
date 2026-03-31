using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

/// <summary>測驗題目實體，支援選擇題與填空題兩種題型</summary>
public class QuizQuestion : BaseEntity
{
    // ==================== 基本資訊 ====================
    public string Content { get; set; } = string.Empty;

    public QuestionType Type { get; set; }
    public int Points { get; set; } = 1;
    public int SortOrder { get; set; }

    /// <summary>填空題的正確答案（僅 FillInBlank 使用）</summary>
    public string? CorrectAnswer { get; set; }

    // ==================== 關聯 ====================
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public ICollection<QuizOption> Options { get; set; } = [];
}
