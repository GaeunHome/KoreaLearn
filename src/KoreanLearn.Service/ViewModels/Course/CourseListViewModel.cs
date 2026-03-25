using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Course;

/// <summary>前台課程列表項目 ViewModel</summary>
public class CourseListViewModel
{
    /// <summary>課程 ID</summary>
    public int Id { get; set; }

    /// <summary>課程標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>課程說明</summary>
    public string? Description { get; set; }

    /// <summary>封面圖片網址</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>課程價格</summary>
    public decimal Price { get; set; }

    /// <summary>難度等級</summary>
    public DifficultyLevel Level { get; set; }

    /// <summary>章節數量</summary>
    public int SectionCount { get; set; }

    /// <summary>單元數量</summary>
    public int LessonCount { get; set; }

    /// <summary>難度等級中文顯示</summary>
    public string LevelDisplay => Level switch
    {
        DifficultyLevel.Beginner => "入門",
        DifficultyLevel.Elementary => "初級",
        DifficultyLevel.Intermediate => "中級",
        DifficultyLevel.Advanced => "進階",
        _ => "未知"
    };
}
