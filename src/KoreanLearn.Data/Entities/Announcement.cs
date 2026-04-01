namespace KoreanLearn.Data.Entities;

/// <summary>公告實體，用於首頁或全站公告訊息</summary>
public class Announcement : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public bool IsPinned { get; set; }
    public int SortOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // ==================== 關聯 ====================
    public ICollection<AnnouncementAttachment> Attachments { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
