using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

public class Course : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public DifficultyLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Discussion> Discussions { get; set; } = [];
}
