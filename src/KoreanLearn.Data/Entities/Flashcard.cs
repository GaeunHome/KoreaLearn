namespace KoreanLearn.Data.Entities;

public class Flashcard : BaseEntity
{
    public int DeckId { get; set; }
    public string Korean { get; set; } = string.Empty;
    public string Chinese { get; set; } = string.Empty;
    public string? Romanization { get; set; }
    public string? ExampleSentence { get; set; }
    public string? AudioUrl { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public FlashcardDeck Deck { get; set; } = null!;
    public ICollection<FlashcardLog> Logs { get; set; } = [];
}
