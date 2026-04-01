namespace KoreanLearn.Service.ViewModels.Admin.Announcement;

/// <summary>公告列表項目 ViewModel（後台管理列表用）</summary>
public class AnnouncementListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPinned { get; set; }
    public int SortOrder { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }
}
