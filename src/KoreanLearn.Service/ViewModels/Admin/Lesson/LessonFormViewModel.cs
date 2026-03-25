using System.ComponentModel.DataAnnotations;
using KoreanLearn.Library.Enums;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.ViewModels.Admin.Lesson;

public class LessonFormViewModel
{
    public int Id { get; set; }

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

    // Video
    [Display(Name = "影片檔案")]
    public IFormFile? VideoFile { get; set; }
    public string? ExistingVideoUrl { get; set; }
    [Display(Name = "影片長度（秒）")]
    public int? VideoDurationSeconds { get; set; }

    // Article
    [Display(Name = "文章內容")]
    public string? ArticleContent { get; set; }

    // PDF
    [Display(Name = "PDF 檔案")]
    public IFormFile? PdfFile { get; set; }
    public string? ExistingPdfUrl { get; set; }
    public string? ExistingPdfFileName { get; set; }

    // 顯示用
    public string? SectionTitle { get; set; }
    public string? CourseTitle { get; set; }
    public int? CourseId { get; set; }
}
