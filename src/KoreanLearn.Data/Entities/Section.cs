namespace KoreanLearn.Data.Entities;

public class Section : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int CourseId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = [];
}
