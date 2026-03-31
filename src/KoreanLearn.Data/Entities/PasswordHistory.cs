namespace KoreanLearn.Data.Entities;

/// <summary>密碼歷史記錄，防止重複使用舊密碼</summary>
public class PasswordHistory
{
    public int Id { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ==================== 關聯 ====================
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
}
