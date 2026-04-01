namespace KoreanLearn.Service.ViewModels.Announcement;

/// <summary>公告詳情 ViewModel（前台詳情頁）</summary>
public class AnnouncementDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<AnnouncementAttachmentViewModel> Attachments { get; set; } = [];
}

/// <summary>公告附件 ViewModel</summary>
public class AnnouncementAttachmentViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileSizeDisplay { get; set; } = string.Empty;
}
