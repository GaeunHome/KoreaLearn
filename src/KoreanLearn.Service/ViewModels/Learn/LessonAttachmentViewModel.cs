namespace KoreanLearn.Service.ViewModels.Learn;

/// <summary>單元附件 ViewModel（用於播放器頁面的附件下載區）</summary>
public class LessonAttachmentViewModel
{
    /// <summary>附件 ID</summary>
    public int Id { get; set; }

    /// <summary>檔案名稱</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>檔案下載網址</summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>格式化後的檔案大小（如 1.2 MB）</summary>
    public string FileSizeDisplay { get; set; } = string.Empty;
}
