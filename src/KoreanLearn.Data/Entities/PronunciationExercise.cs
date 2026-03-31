namespace KoreanLearn.Data.Entities;

/// <summary>發音練習實體，包含標準韓文發音音檔供學生跟讀練習</summary>
public class PronunciationExercise : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Korean { get; set; } = string.Empty;

    public string? Romanization { get; set; }

    public string? Chinese { get; set; }

    public string StandardAudioUrl { get; set; } = string.Empty;

    // ==================== 關聯 ====================
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public ICollection<PronunciationAttempt> Attempts { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
