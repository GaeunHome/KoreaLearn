using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Admin.Course;

/// <summary>後台課程列表項目 ViewModel</summary>
public class CourseAdminListViewModel
{
    /// <summary>課程 ID</summary>
    public int Id { get; set; }

    /// <summary>課程標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>售價</summary>
    public decimal Price { get; set; }

    /// <summary>難度等級</summary>
    public DifficultyLevel Level { get; set; }

    /// <summary>是否已發佈</summary>
    public bool IsPublished { get; set; }

    /// <summary>排序權重</summary>
    public int SortOrder { get; set; }

    /// <summary>章節數量</summary>
    public int SectionCount { get; set; }

    /// <summary>單元數量</summary>
    public int LessonCount { get; set; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }

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
