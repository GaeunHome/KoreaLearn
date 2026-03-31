namespace KoreanLearn.Data.Entities;

/// <summary>學習進度實體，記錄使用者對每個單元的完成狀態與影片播放進度</summary>
public class Progress : BaseEntity
{
    // ==================== 基本資訊 ====================
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>影片播放進度（秒），用於續播功能</summary>
    public int VideoProgressSeconds { get; set; }

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
}
