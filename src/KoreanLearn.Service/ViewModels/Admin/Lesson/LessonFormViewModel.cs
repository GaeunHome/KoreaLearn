using System.ComponentModel.DataAnnotations;
using KoreanLearn.Library.Enums;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.ViewModels.Admin.Lesson;

/// <summary>建立/編輯單元表單 ViewModel（支援影片、文章、PDF 三種類型）</summary>
public class LessonFormViewModel
{
    /// <summary>單元 ID（編輯時使用）</summary>
    public int Id { get; set; }

    /// <summary>所屬章節 ID</summary>
    public int SectionId { get; set; }

    [Required(ErrorMessage = "單元標題為必填")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題須介於 1–200 字元")]
    [Display(Name = "單元標題")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "單元說明")]
    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "請選擇類型")]
    [Display(Name = "類型")]
    public LessonType Type { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }

    [Display(Name = "免費試看")]
    public bool IsFreePreview { get; set; }

    // ── 影片相關 ──

    [Display(Name = "影片檔案")]
    public IFormFile? VideoFile { get; set; }

    /// <summary>現有影片網址（編輯時保留）</summary>
    public string? ExistingVideoUrl { get; set; }

    [Display(Name = "影片長度（秒）")]
    public int? VideoDurationSeconds { get; set; }

    // ── 文章相關 ──

    [Display(Name = "文章內容")]
    public string? ArticleContent { get; set; }

    // ── PDF 相關 ──

    [Display(Name = "PDF 檔案")]
    public IFormFile? PdfFile { get; set; }

    /// <summary>現有 PDF 網址（編輯時保留）</summary>
    public string? ExistingPdfUrl { get; set; }

    /// <summary>現有 PDF 檔案名稱</summary>
    public string? ExistingPdfFileName { get; set; }

    // ── 頁面導覽資訊 ──

    /// <summary>所屬章節標題（顯示用）</summary>
    public string? SectionTitle { get; set; }

    /// <summary>所屬課程標題（顯示用）</summary>
    public string? CourseTitle { get; set; }

    /// <summary>所屬課程 ID（導覽用）</summary>
    public int? CourseId { get; set; }
}
