using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.ViewModels.Admin.Pronunciation;

/// <summary>發音練習列表項目 ViewModel</summary>
public class PronunciationListViewModel
{
    /// <summary>練習 ID</summary>
    public int Id { get; set; }

    /// <summary>韓文</summary>
    public string Korean { get; set; } = string.Empty;

    /// <summary>羅馬拼音</summary>
    public string? Romanization { get; set; }

    /// <summary>中文</summary>
    public string? Chinese { get; set; }

    /// <summary>標準音檔網址</summary>
    public string? StandardAudioUrl { get; set; }

    /// <summary>關聯單元 ID</summary>
    public int? LessonId { get; set; }
}

/// <summary>建立/編輯發音練習表單 ViewModel</summary>
public class PronunciationFormViewModel
{
    /// <summary>練習 ID（編輯時使用）</summary>
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

    /// <summary>現有音檔網址（編輯時保留）</summary>
    public string? ExistingAudioUrl { get; set; }

    [Display(Name = "關聯單元")]
    public int? LessonId { get; set; }
}
