namespace KoreanLearn.Service.ViewModels.Learn;

public class PdfPlayerViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PdfUrl { get; set; }
    public string? PdfFileName { get; set; }
    public bool IsCompleted { get; set; }

    public int SectionId { get; set; }
    public string? SectionTitle { get; set; }
    public int CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public int? PreviousLessonId { get; set; }
    public int? NextLessonId { get; set; }
}
