using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

/// <summary>課程實體，代表一門韓語課程</summary>
public class Course : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CoverImageUrl { get; set; }

    public decimal Price { get; set; }

    public DifficultyLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }

    // ==================== 關聯 ====================
    public string? TeacherId { get; set; }
    public AppUser? Teacher { get; set; }

    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Discussion> Discussions { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
