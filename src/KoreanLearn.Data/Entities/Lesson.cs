using KoreanLearn.Library.Enums;

namespace KoreanLearn.Data.Entities;

/// <summary>
/// 單元實體，代表章節下的一個學習單元。
/// 根據 Type 欄位決定使用哪組條件欄位：Video / Article / Pdf
/// </summary>
public class Lesson : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public LessonType Type { get; set; }
    public int SortOrder { get; set; }
    public bool IsFreePreview { get; set; }

    // ==================== 條件欄位（依 Type 使用）====================
    public string? VideoUrl { get; set; }
    public int? VideoDurationSeconds { get; set; }

    public string? ArticleContent { get; set; }

    public string? PdfUrl { get; set; }

    public string? PdfFileName { get; set; }

    // ==================== 關聯 ====================
    public int SectionId { get; set; }
    public Section Section { get; set; } = null!;

    public ICollection<Progress> Progresses { get; set; } = [];
    public ICollection<LessonAttachment> Attachments { get; set; } = [];
    public Quiz? Quiz { get; set; }

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
