namespace KoreanLearn.Data.Entities;

/// <summary>單元附件實體，儲存單元的補充教材檔案</summary>
public class LessonAttachment : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
    public int SortOrder { get; set; }

    // ==================== 關聯 ====================
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
