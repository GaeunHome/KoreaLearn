using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Course;

public class CourseDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public DifficultyLevel Level { get; set; }
    public string? TeacherName { get; set; }
    public bool IsEnrolled { get; set; }
    public IReadOnlyList<SectionViewModel> Sections { get; set; } = [];

    public string LevelDisplay => Level switch
    {
        DifficultyLevel.Beginner => "入門",
        DifficultyLevel.Elementary => "初級",
        DifficultyLevel.Intermediate => "中級",
        DifficultyLevel.Advanced => "進階",
        _ => "未知"
    };

    public int TotalLessons => Sections.Sum(s => s.Lessons.Count);
    public int CompletedLessons => Sections.Sum(s => s.Lessons.Count(l => l.IsCompleted));
    public int ProgressPercent => TotalLessons > 0 ? CompletedLessons * 100 / TotalLessons : 0;
}

public class SectionViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<LessonSummaryViewModel> Lessons { get; set; } = [];
}

public class LessonSummaryViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public LessonType Type { get; set; }
    public bool IsFreePreview { get; set; }
    public bool IsCompleted { get; set; }

    public string TypeIcon => Type switch
    {
        LessonType.Video => "bi-play-circle",
        LessonType.Article => "bi-file-text",
        LessonType.Pdf => "bi-file-pdf",
        _ => "bi-file"
    };

    public string TypeDisplay => Type switch
    {
        LessonType.Video => "影片",
        LessonType.Article => "文章",
        LessonType.Pdf => "PDF",
        _ => "未知"
    };
}
