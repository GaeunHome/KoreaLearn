namespace KoreanLearn.Service.ViewModels.Admin.SystemSetting;

/// <summary>系統參數列表顯示 ViewModel</summary>
public class SystemSettingViewModel
{
    /// <summary>主鍵識別碼</summary>
    public int Id { get; set; }

    /// <summary>參數識別鍵</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>參數值</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>參數說明</summary>
    public string? Description { get; set; }

    /// <summary>分組名稱</summary>
    public string? Group { get; set; }

    /// <summary>最後更新時間</summary>
    public DateTime UpdatedAt { get; set; }
}
