namespace KoreanLearn.Service.ViewModels.Learn;

/// <summary>PDF 閱讀器頁面 ViewModel（含下載連結、導覽與附件）</summary>
public class PdfPlayerViewModel
{
    /// <summary>單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>單元標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>單元說明</summary>
    public string? Description { get; set; }

    /// <summary>PDF 檔案網址</summary>
    public string? PdfUrl { get; set; }

    /// <summary>PDF 檔案名稱</summary>
    public string? PdfFileName { get; set; }

    /// <summary>是否已完成此單元</summary>
    public bool IsCompleted { get; set; }

    /// <summary>所屬章節 ID</summary>
    public int SectionId { get; set; }

    /// <summary>所屬章節標題</summary>
    public string? SectionTitle { get; set; }

    /// <summary>所屬課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>所屬課程標題</summary>
    public string? CourseTitle { get; set; }

    /// <summary>上一個單元 ID</summary>
    public int? PreviousLessonId { get; set; }

    /// <summary>上一個單元的 Action 名稱（Video/Article/Pdf）</summary>
    public string? PreviousLessonAction { get; set; }

    /// <summary>下一個單元 ID</summary>
    public int? NextLessonId { get; set; }

    /// <summary>下一個單元的 Action 名稱（Video/Article/Pdf）</summary>
    public string? NextLessonAction { get; set; }

    /// <summary>附件列表</summary>
    public IReadOnlyList<LessonAttachmentViewModel> Attachments { get; set; } = [];
}
