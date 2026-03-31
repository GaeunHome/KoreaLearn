namespace KoreanLearn.Service.ViewModels.Admin.Banner;

/// <summary>幻燈片列表顯示 ViewModel</summary>
public class BannerViewModel
{
    /// <summary>主鍵識別碼</summary>
    public int Id { get; set; }

    /// <summary>標題</summary>
    public string? Title { get; set; }

    /// <summary>圖片 URL</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>連結課程 ID</summary>
    public int? CourseId { get; set; }

    /// <summary>連結課程標題</summary>
    public string? CourseTitle { get; set; }

    /// <summary>顯示順序</summary>
    public int DisplayOrder { get; set; }

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; }
}
