namespace KoreanLearn.Data.Entities;

/// <summary>字卡實體，記錄韓文詞彙的韓文、中文翻譯與羅馬拼音</summary>
public class Flashcard : BaseEntity
{
    // ==================== 基本資訊 ====================
    public string Korean { get; set; } = string.Empty;

    public string Chinese { get; set; } = string.Empty;

    public string? Romanization { get; set; }

    public string? ExampleSentence { get; set; }

    public string? AudioUrl { get; set; }

    public int SortOrder { get; set; }

    // ==================== 關聯 ====================
    public int DeckId { get; set; }
    public FlashcardDeck Deck { get; set; } = null!;

    public ICollection<FlashcardLog> Logs { get; set; } = [];
}
