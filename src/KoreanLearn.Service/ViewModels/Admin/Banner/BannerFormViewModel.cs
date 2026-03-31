using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KoreanLearn.Service.ViewModels.Admin.Banner;

/// <summary>幻燈片新增/編輯表單 ViewModel</summary>
public class BannerFormViewModel
{
    /// <summary>主鍵識別碼（編輯時使用）</summary>
    public int Id { get; set; }

    /// <summary>標題</summary>
    [StringLength(100, ErrorMessage = "標題長度不可超過 100 字元")]
    [Display(Name = "標題")]
    public string? Title { get; set; }

    /// <summary>圖片 URL</summary>
    [Display(Name = "圖片路徑")]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>連結課程 ID</summary>
    [Display(Name = "連結課程")]
    public int? CourseId { get; set; }

    /// <summary>顯示順序</summary>
    [Display(Name = "顯示順序")]
    [Range(0, 999, ErrorMessage = "顯示順序必須在 0 ~ 999 之間")]
    public int DisplayOrder { get; set; }

    /// <summary>是否啟用</summary>
    [Display(Name = "是否啟用")]
    public bool IsActive { get; set; } = true;

    /// <summary>上傳圖片檔案</summary>
    [Display(Name = "上傳圖片")]
    public IFormFile? ImageFile { get; set; }
}
