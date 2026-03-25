using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Admin.Flashcard;

public class DeckListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public int CardCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeckFormViewModel
{
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

public class DeckDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public IReadOnlyList<CardViewModel> Cards { get; set; } = [];
}

public class CardViewModel
{
    public int Id { get; set; }
    public string Korean { get; set; } = string.Empty;
    public string Chinese { get; set; } = string.Empty;
    public string? Romanization { get; set; }
    public string? ExampleSentence { get; set; }
    public int SortOrder { get; set; }
}

public class CardFormViewModel
{
    public int Id { get; set; }
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

    // Context
    public string? DeckTitle { get; set; }
}
