namespace KoreanLearn.Data.Entities;

/// <summary>測驗作答紀錄實體，記錄學生的一次測驗嘗試與成績</summary>
public class QuizAttempt : BaseEntity
{
    // ==================== 基本資訊 ====================
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public bool IsPassed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public ICollection<QuizAnswer> Answers { get; set; } = [];
}
