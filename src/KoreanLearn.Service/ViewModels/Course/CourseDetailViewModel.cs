using KoreanLearn.Library.Enums;

namespace KoreanLearn.Service.ViewModels.Course;

/// <summary>前台課程詳情頁 ViewModel（含章節、單元列表與學習進度）</summary>
public class CourseDetailViewModel
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

    /// <summary>授課教師名稱</summary>
    public string? TeacherName { get; set; }

    /// <summary>目前使用者是否已選此課程</summary>
    public bool IsEnrolled { get; set; }

    /// <summary>章節列表（含各章節的單元）</summary>
    public IReadOnlyList<SectionViewModel> Sections { get; set; } = [];

    /// <summary>難度等級中文顯示</summary>
    public string LevelDisplay => Level switch
    {
        DifficultyLevel.Beginner => "入門",
        DifficultyLevel.Elementary => "初級",
        DifficultyLevel.Intermediate => "中級",
        DifficultyLevel.Advanced => "進階",
        _ => "未知"
    };

    /// <summary>課程總單元數</summary>
    public int TotalLessons => Sections.Sum(s => s.Lessons.Count);

    /// <summary>已完成單元數</summary>
    public int CompletedLessons => Sections.Sum(s => s.Lessons.Count(l => l.IsCompleted));

    /// <summary>學習進度百分比</summary>
    public int ProgressPercent => TotalLessons > 0 ? CompletedLessons * 100 / TotalLessons : 0;
}

/// <summary>章節 ViewModel（用於課程詳情頁的章節列表）</summary>
public class SectionViewModel
{
    /// <summary>章節 ID</summary>
    public int Id { get; set; }

    /// <summary>章節標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>章節說明</summary>
    public string? Description { get; set; }

    /// <summary>章節內的單元列表</summary>
    public IReadOnlyList<LessonSummaryViewModel> Lessons { get; set; } = [];
}

/// <summary>單元摘要 ViewModel（用於課程詳情頁的單元列表項目）</summary>
public class LessonSummaryViewModel
{
    /// <summary>單元 ID</summary>
    public int Id { get; set; }

    /// <summary>單元標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>單元類型（影片/文章/PDF）</summary>
    public LessonType Type { get; set; }

    /// <summary>是否為免費試看</summary>
    public bool IsFreePreview { get; set; }

    /// <summary>是否已完成</summary>
    public bool IsCompleted { get; set; }

    /// <summary>單元類型對應的 Bootstrap Icon CSS 類別</summary>
    public string TypeIcon => Type.ToIcon();

    /// <summary>單元類型中文顯示</summary>
    public string TypeDisplay => Type.ToDisplay();
}
