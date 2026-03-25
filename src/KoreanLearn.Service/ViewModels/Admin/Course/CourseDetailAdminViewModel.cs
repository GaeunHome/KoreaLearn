using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Admin.Course;

public class CourseDetailAdminViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public DifficultyLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
    public IReadOnlyList<SectionAdminViewModel> Sections { get; set; } = [];

    public string LevelDisplay => Level switch
    {
        DifficultyLevel.Beginner => "入門",
        DifficultyLevel.Elementary => "初級",
        DifficultyLevel.Intermediate => "中級",
        DifficultyLevel.Advanced => "進階",
        _ => "未知"
    };
}

public class SectionAdminViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int CourseId { get; set; }
    public IReadOnlyList<LessonAdminViewModel> Lessons { get; set; } = [];
}

public class LessonAdminViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public LessonType Type { get; set; }
    public int SortOrder { get; set; }
    public int SectionId { get; set; }
    public bool IsFreePreview { get; set; }

    public string TypeDisplay => Type switch
    {
        LessonType.Video => "影片",
        LessonType.Article => "文章",
        LessonType.Pdf => "PDF",
        _ => "未知"
    };

    public string TypeIcon => Type switch
    {
        LessonType.Video => "bi-play-circle",
        LessonType.Article => "bi-file-text",
        LessonType.Pdf => "bi-file-pdf",
        _ => "bi-file"
    };
}
