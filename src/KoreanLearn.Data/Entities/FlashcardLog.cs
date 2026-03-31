namespace KoreanLearn.Data.Entities;

/// <summary>
/// 字卡學習紀錄實體，使用 SM-2 間隔重複演算法追蹤每張字卡的複習排程。
/// Quality（0-5）為使用者自評的記憶品質，EaseFactor 控制間隔增長速度。
/// </summary>
public class FlashcardLog : BaseEntity
{
    // ==================== SM-2 演算法欄位 ====================
    /// <summary>品質評分（0=完全忘記，5=完美記憶）</summary>
    public int Quality { get; set; }

    /// <summary>難易度因子（初始 2.5）</summary>
    public double EaseFactor { get; set; } = 2.5;

    /// <summary>複習間隔天數</summary>
    public int Interval { get; set; } = 1;

    /// <summary>連續正確作答次數</summary>
    public int Repetition { get; set; }

    /// <summary>下次複習日期</summary>
    public DateTime NextReviewDate { get; set; }

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int FlashcardId { get; set; }
    public Flashcard Flashcard { get; set; } = null!;
}
