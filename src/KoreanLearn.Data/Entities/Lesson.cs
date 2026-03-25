using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

public class Lesson : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public int SortOrder { get; set; }
    public int SectionId { get; set; }
    public bool IsFreePreview { get; set; }

    // 根據 Type 只有一個會有值
    public string? VideoUrl { get; set; }           // LessonType.Video
    public int? VideoDurationSeconds { get; set; }   // LessonType.Video
    public string? ArticleContent { get; set; }      // LessonType.Article
    public string? PdfUrl { get; set; }              // LessonType.Pdf
    public string? PdfFileName { get; set; }         // LessonType.Pdf

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Section Section { get; set; } = null!;
    public ICollection<Progress> Progresses { get; set; } = [];
    public Quiz? Quiz { get; set; }
}
