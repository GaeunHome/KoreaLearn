using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Admin.Section;

public class SectionFormViewModel
{
    public int Id { get; set; }

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

    // 顯示用
    public string? CourseTitle { get; set; }
}
