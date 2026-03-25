namespace KoreanLearn.Data.Entities;

public class Discussion : BaseEntity, ISoftDeletable
{
    public string UserId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<DiscussionReply> Replies { get; set; } = [];
}
