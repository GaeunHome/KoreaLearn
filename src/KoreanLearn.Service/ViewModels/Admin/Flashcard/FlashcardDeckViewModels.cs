using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Admin.Flashcard;

/// <summary>後台牌組列表項目 ViewModel</summary>
public class DeckListViewModel
{
    /// <summary>牌組 ID</summary>
    public int Id { get; set; }

    /// <summary>牌組標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>牌組說明</summary>
    public string? Description { get; set; }

    /// <summary>關聯課程 ID</summary>
    public int? CourseId { get; set; }

    /// <summary>關聯課程標題</summary>
    public string? CourseTitle { get; set; }

    /// <summary>字卡數量</summary>
    public int CardCount { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>建立/編輯牌組表單 ViewModel</summary>
public class DeckFormViewModel
{
    /// <summary>牌組 ID（編輯時使用）</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, MinimumLength = 1)]
    [Display(Name = "牌組標題")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "說明")]
    [MaxLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "關聯課程")]
    public int? CourseId { get; set; }
}

/// <summary>後台牌組詳情 ViewModel（含所有字卡）</summary>
public class DeckDetailViewModel
{
    /// <summary>牌組 ID</summary>
    public int Id { get; set; }

    /// <summary>牌組標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>牌組說明</summary>
    public string? Description { get; set; }

    /// <summary>關聯課程 ID</summary>
    public int? CourseId { get; set; }

    /// <summary>關聯課程標題</summary>
    public string? CourseTitle { get; set; }

    /// <summary>字卡列表</summary>
    public IReadOnlyList<CardViewModel> Cards { get; set; } = [];
}

/// <summary>字卡 ViewModel（顯示用）</summary>
public class CardViewModel
{
    /// <summary>字卡 ID</summary>
    public int Id { get; set; }

    /// <summary>韓文</summary>
    public string Korean { get; set; } = string.Empty;

    /// <summary>中文</summary>
    public string Chinese { get; set; } = string.Empty;

    /// <summary>羅馬拼音</summary>
    public string? Romanization { get; set; }

    /// <summary>例句</summary>
    public string? ExampleSentence { get; set; }

    /// <summary>排序權重</summary>
    public int SortOrder { get; set; }
}

/// <summary>建立/編輯字卡表單 ViewModel</summary>
public class CardFormViewModel
{
    /// <summary>字卡 ID（編輯時使用）</summary>
    public int Id { get; set; }

    /// <summary>所屬牌組 ID</summary>
    public int DeckId { get; set; }

    [Required(ErrorMessage = "韓文為必填")]
    [StringLength(200)]
    [Display(Name = "韓文")]
    public string Korean { get; set; } = string.Empty;

    [Required(ErrorMessage = "中文為必填")]
    [StringLength(200)]
    [Display(Name = "中文")]
    public string Chinese { get; set; } = string.Empty;

    [Display(Name = "羅馬拼音")]
    [StringLength(200)]
    public string? Romanization { get; set; }

    [Display(Name = "例句")]
    [MaxLength(1000)]
    public string? ExampleSentence { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }

    /// <summary>所屬牌組標題（顯示用）</summary>
    public string? DeckTitle { get; set; }
}
