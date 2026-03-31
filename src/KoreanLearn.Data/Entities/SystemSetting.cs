namespace KoreanLearn.Data.Entities;

/// <summary>系統參數設定（Key-Value 結構，由 Admin 透過後台管理）</summary>
public class SystemSetting
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Group { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
