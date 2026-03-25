namespace KoreanLearn.Service.ViewModels.Learn;

/// <summary>字卡學習 Session ViewModel（含待複習與新卡片）</summary>
public class FlashcardStudyViewModel
{
    /// <summary>牌組 ID</summary>
    public int DeckId { get; set; }

    /// <summary>牌組標題</summary>
    public string DeckTitle { get; set; } = string.Empty;

    /// <summary>本次學習的字卡列表（最多 20 張）</summary>
    public IReadOnlyList<FlashcardItemViewModel> Cards { get; set; } = [];

    /// <summary>牌組內的字卡總數</summary>
    public int TotalCards { get; set; }

    /// <summary>待複習的卡片數</summary>
    public int DueCards { get; set; }

    /// <summary>尚未學過的新卡片數</summary>
    public int NewCards { get; set; }
}

/// <summary>字卡學習項目 ViewModel（翻轉卡片的正反面資料）</summary>
public class FlashcardItemViewModel
{
    /// <summary>字卡 ID</summary>
    public int CardId { get; set; }

    /// <summary>韓文（正面）</summary>
    public string Korean { get; set; } = string.Empty;

    /// <summary>中文（背面）</summary>
    public string Chinese { get; set; } = string.Empty;

    /// <summary>羅馬拼音</summary>
    public string? Romanization { get; set; }

    /// <summary>例句</summary>
    public string? ExampleSentence { get; set; }

    /// <summary>是否為首次出現的新卡片</summary>
    public bool IsNew { get; set; }
}

/// <summary>字卡牌組列表 ViewModel（前台選擇牌組用）</summary>
public class FlashcardDeckListViewModel
{
    /// <summary>牌組 ID</summary>
    public int Id { get; set; }

    /// <summary>牌組標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>牌組說明</summary>
    public string? Description { get; set; }

    /// <summary>字卡總數</summary>
    public int CardCount { get; set; }

    /// <summary>待複習的卡片數</summary>
    public int DueCount { get; set; }
}
