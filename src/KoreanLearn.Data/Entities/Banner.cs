namespace KoreanLearn.Data.Entities;

/// <summary>首頁幻燈片橫幅</summary>
public class Banner : BaseEntity, ISoftDeletable
{
    // ==================== 基本資訊 ====================
    public string? Title { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // ==================== 關聯 ====================
    public int? CourseId { get; set; }
    public Course? Course { get; set; }

    // ==================== 軟刪除 ====================
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
