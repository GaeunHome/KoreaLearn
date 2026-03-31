namespace KoreanLearn.Data.Entities;

/// <summary>測驗實體，代表一個單元對應的測驗</summary>
public class Quiz : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int PassingScore { get; set; } = 70;

    /// <summary>作答時間限制（分鐘），0 表示不限時</summary>
    public int TimeLimitMinutes { get; set; }

    // ==================== 關聯 ====================
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public ICollection<QuizQuestion> Questions { get; set; } = [];
    public ICollection<QuizAttempt> Attempts { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
