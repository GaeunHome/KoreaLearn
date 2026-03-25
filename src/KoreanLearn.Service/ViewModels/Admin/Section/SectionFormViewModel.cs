using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Admin.Section;

/// <summary>建立/編輯章節表單 ViewModel</summary>
public class SectionFormViewModel
{
    /// <summary>章節 ID（編輯時使用）</summary>
    public int Id { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    [Required(ErrorMessage = "章節標題為必填")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題須介於 1–200 字元")]
    [Display(Name = "章節標題")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "章節說明")]
    [MaxLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }

    /// <summary>所屬課程標題（顯示用）</summary>
    public string? CourseTitle { get; set; }
}
