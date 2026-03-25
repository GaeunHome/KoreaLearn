namespace KoreanLearn.Data.Entities;

public class FlashcardLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int FlashcardId { get; set; }
    public int Quality { get; set; }        // SM-2: 0-5 品質評分
    public double EaseFactor { get; set; } = 2.5;
    public int Interval { get; set; } = 1;  // 天數
    public int Repetition { get; set; }
    public DateTime NextReviewDate { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Flashcard Flashcard { get; set; } = null!;
}
