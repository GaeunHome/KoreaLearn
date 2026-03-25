using System.ComponentModel.DataAnnotations;
using KoreanLearn.Library.Enums;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.ViewModels.Admin.Course;

public class CreateCourseViewModel
{
    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題須介於 1–200 字元")]
    [Display(Name = "課程標題")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "課程介紹")]
    [MaxLength(4000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "價格為必填")]
    [Range(0, 99999.99, ErrorMessage = "價格須在合理範圍內")]
    [Display(Name = "售價")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "請選擇難度等級")]
    [Display(Name = "難度等級")]
    public DifficultyLevel Level { get; set; }

    [Display(Name = "封面圖片")]
    public IFormFile? CoverImage { get; set; }

    [Display(Name = "是否發佈")]
    public bool IsPublished { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }
}
