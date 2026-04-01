namespace KoreanLearn.Service.ViewModels.Announcement;

/// <summary>公告卡片 ViewModel（前台列表用）</summary>
public class AnnouncementCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentPreview { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public bool HasAttachment { get; set; }
    public DateTime CreatedAt { get; set; }
}
