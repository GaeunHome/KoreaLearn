using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Admin.Course;

/// <summary>後台課程詳情 ViewModel（含章節與單元列表）</summary>
public class CourseDetailAdminViewModel
{
    /// <summary>課程 ID</summary>
    public int Id { get; set; }

    /// <summary>課程標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>課程說明</summary>
    public string? Description { get; set; }

    /// <summary>封面圖片網址</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>售價</summary>
    public decimal Price { get; set; }

    /// <summary>難度等級</summary>
    public DifficultyLevel Level { get; set; }

    /// <summary>是否已發佈</summary>
    public bool IsPublished { get; set; }

    /// <summary>排序權重</summary>
    public int SortOrder { get; set; }

    /// <summary>章節列表</summary>
    public IReadOnlyList<SectionAdminViewModel> Sections { get; set; } = [];

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

/// <summary>後台章節 ViewModel（含單元列表）</summary>
public class SectionAdminViewModel
{
    /// <summary>章節 ID</summary>
    public int Id { get; set; }

    /// <summary>章節標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>章節說明</summary>
    public string? Description { get; set; }

    /// <summary>排序權重</summary>
    public int SortOrder { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>單元列表</summary>
    public IReadOnlyList<LessonAdminViewModel> Lessons { get; set; } = [];
}

/// <summary>後台單元 ViewModel</summary>
public class LessonAdminViewModel
{
    /// <summary>單元 ID</summary>
    public int Id { get; set; }

    /// <summary>單元標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>單元類型</summary>
    public LessonType Type { get; set; }

    /// <summary>排序權重</summary>
    public int SortOrder { get; set; }

    /// <summary>所屬章節 ID</summary>
    public int SectionId { get; set; }

    /// <summary>是否為免費試看</summary>
    public bool IsFreePreview { get; set; }

    /// <summary>類型中文顯示</summary>
    public string TypeDisplay => Type.ToDisplay();

    /// <summary>類型對應的 Bootstrap Icon CSS 類別</summary>
    public string TypeIcon => Type.ToIcon();
}
