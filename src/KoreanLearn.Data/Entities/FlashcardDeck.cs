namespace KoreanLearn.Data.Entities;

/// <summary>字卡牌組實體，將多張字卡組織成一個主題牌組</summary>
public class FlashcardDeck : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    // ==================== 關聯 ====================
    /// <summary>關聯課程 ID（可選，null 表示獨立牌組）</summary>
    public int? CourseId { get; set; }
    public Course? Course { get; set; }

    public ICollection<Flashcard> Flashcards { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
