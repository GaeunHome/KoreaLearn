namespace KoreanLearn.Data.Entities;

/// <summary>公告附件實體，儲存公告的附加檔案</summary>
public class AnnouncementAttachment : BaseEntity
{
    // ==================== 基本資訊 ====================
    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
    public int SortOrder { get; set; }

    // ==================== 關聯 ====================
    public int AnnouncementId { get; set; }
    public Announcement Announcement { get; set; } = null!;
}
