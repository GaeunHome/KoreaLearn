namespace KoreanLearn.Service.ViewModels.Learn;

public class ArticlePlayerViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ArticleContent { get; set; }
    public bool IsCompleted { get; set; }

    // Navigation context
    public int SectionId { get; set; }
    public string? SectionTitle { get; set; }
    public int CourseId { get; set; }
    public string? CourseTitle { get; set; }

    // Next/Previous lesson
    public int? PreviousLessonId { get; set; }
    public int? NextLessonId { get; set; }
    public IReadOnlyList<LessonAttachmentViewModel> Attachments { get; set; } = [];
}
