namespace KoreanLearn.Service.ViewModels.Learn;

public class FlashcardStudyViewModel
{
    public int DeckId { get; set; }
    public string DeckTitle { get; set; } = string.Empty;
    public IReadOnlyList<FlashcardItemViewModel> Cards { get; set; } = [];
    public int TotalCards { get; set; }
    public int DueCards { get; set; }
    public int NewCards { get; set; }
}

public class FlashcardItemViewModel
{
    public int CardId { get; set; }
    public string Korean { get; set; } = string.Empty;
    public string Chinese { get; set; } = string.Empty;
    public string? Romanization { get; set; }
    public string? ExampleSentence { get; set; }
    public bool IsNew { get; set; }
}

public class FlashcardDeckListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CardCount { get; set; }
    public int DueCount { get; set; }
}
