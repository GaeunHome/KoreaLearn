namespace KoreanLearn.Data.Entities;

public class DiscussionReply : BaseEntity, ISoftDeletable
{
    public string UserId { get; set; } = string.Empty;
    public int DiscussionId { get; set; }
    public string Content { get; set; } = string.Empty;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Discussion Discussion { get; set; } = null!;
}
