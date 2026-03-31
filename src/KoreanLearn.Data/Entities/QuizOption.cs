namespace KoreanLearn.Data.Entities;

/// <summary>測驗選項實體，代表選擇題中的一個選項</summary>
public class QuizOption : BaseEntity
{
    // ==================== 基本資訊 ====================
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }

    // ==================== 關聯 ====================
    public int QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;
}
