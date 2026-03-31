namespace KoreanLearn.Data.Entities;

/// <summary>章節實體，代表課程下的一個章節</summary>
public class Section : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    // ==================== 關聯 ====================
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = [];

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
