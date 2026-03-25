using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.ViewModels.Admin.Pronunciation;

public class PronunciationListViewModel
{
    public int Id { get; set; }
    public string Korean { get; set; } = string.Empty;
    public string? Romanization { get; set; }
    public string? Chinese { get; set; }
    public string? StandardAudioUrl { get; set; }
    public int? LessonId { get; set; }
}

public class PronunciationFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "韓文為必填")]
    [StringLength(200)]
    [Display(Name = "韓文")]
    public string Korean { get; set; } = string.Empty;

    [Display(Name = "羅馬拼音")]
    [StringLength(200)]
    public string? Romanization { get; set; }

    [Display(Name = "中文")]
    [StringLength(200)]
    public string? Chinese { get; set; }

    [Display(Name = "標準音檔")]
    public IFormFile? AudioFile { get; set; }
    public string? ExistingAudioUrl { get; set; }

    [Display(Name = "關聯單元")]
    public int? LessonId { get; set; }
}
