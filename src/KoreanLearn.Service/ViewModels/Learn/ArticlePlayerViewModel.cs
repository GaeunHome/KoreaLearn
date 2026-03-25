namespace KoreanLearn.Service.ViewModels.Learn;

/// <summary>文章閱讀器頁面 ViewModel（含文章內容、導覽與附件）</summary>
public class ArticlePlayerViewModel
{
    /// <summary>單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>單元標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>單元說明</summary>
    public string? Description { get; set; }

    /// <summary>文章 HTML 內容</summary>
    public string? ArticleContent { get; set; }

    /// <summary>是否已完成此單元</summary>
    public bool IsCompleted { get; set; }

    // ── 導覽資訊 ──

    /// <summary>所屬章節 ID</summary>
    public int SectionId { get; set; }

    /// <summary>所屬章節標題</summary>
    public string? SectionTitle { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>所屬課程標題</summary>
    public string? CourseTitle { get; set; }

    // ── 前後單元 ──

    /// <summary>上一個單元 ID</summary>
    public int? PreviousLessonId { get; set; }

    /// <summary>下一個單元 ID</summary>
    public int? NextLessonId { get; set; }

    /// <summary>附件列表</summary>
    public IReadOnlyList<LessonAttachmentViewModel> Attachments { get; set; } = [];
}
