namespace KoreanLearn.Data.Entities;

/// <summary>討論區回覆實體，代表對討論主題的一則回覆</summary>
public class DiscussionReply : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Content { get; set; } = string.Empty;

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int DiscussionId { get; set; }
    public Discussion Discussion { get; set; } = null!;

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
