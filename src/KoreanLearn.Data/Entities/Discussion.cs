namespace KoreanLearn.Data.Entities;

/// <summary>討論區主題實體，代表課程討論區中的一則討論</summary>
public class Discussion : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<DiscussionReply> Replies { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
