namespace KoreanLearn.Service.ViewModels.Learn;

/// <summary>影片播放器頁面 ViewModel（含播放進度、導覽與附件）</summary>
public class VideoPlayerViewModel
{
    /// <summary>單元 ID</summary>
    public int LessonId { get; set; }

    /// <summary>單元標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>單元說明</summary>
    public string? Description { get; set; }

    /// <summary>影片檔案網址</summary>
    public string? VideoUrl { get; set; }

    /// <summary>影片總時長（秒）</summary>
    public int? VideoDurationSeconds { get; set; }

    /// <summary>使用者上次播放進度（秒）</summary>
    public int VideoProgressSeconds { get; set; }

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

    // ── 附件 ──

    /// <summary>附件列表</summary>
    public IReadOnlyList<LessonAttachmentViewModel> Attachments { get; set; } = [];
}
