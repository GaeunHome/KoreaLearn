using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Admin.Course;

public class CourseAdminListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DifficultyLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
    public int SectionCount { get; set; }
    public int LessonCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public string LevelDisplay => Level switch
    {
        DifficultyLevel.Beginner => "入門",
        DifficultyLevel.Elementary => "初級",
        DifficultyLevel.Intermediate => "中級",
        DifficultyLevel.Advanced => "進階",
        _ => "未知"
    };
}
