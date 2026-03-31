namespace KoreanLearn.Data.Entities;

/// <summary>發音練習嘗試紀錄實體，記錄學生上傳的錄音檔</summary>
public class PronunciationAttempt : BaseEntity
{
    // ==================== 基本資訊 ====================
    public string RecordingUrl { get; set; } = string.Empty;

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int ExerciseId { get; set; }
    public PronunciationExercise Exercise { get; set; } = null!;
}
