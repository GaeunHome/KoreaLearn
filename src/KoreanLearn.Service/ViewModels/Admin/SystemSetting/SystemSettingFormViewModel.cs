using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Admin.SystemSetting;

/// <summary>系統參數新增/編輯表單 ViewModel</summary>
public class SystemSettingFormViewModel
{
    /// <summary>主鍵識別碼（編輯時使用）</summary>
    public int Id { get; set; }

    /// <summary>參數識別鍵</summary>
    [Required(ErrorMessage = "請輸入參數鍵")]
    [StringLength(100, ErrorMessage = "參數鍵長度不可超過 100 字元")]
    [Display(Name = "參數鍵")]
    public string Key { get; set; } = string.Empty;

    /// <summary>參數值</summary>
    [Required(ErrorMessage = "請輸入參數值")]
    [StringLength(2000, ErrorMessage = "參數值長度不可超過 2000 字元")]
    [Display(Name = "參數值")]
    public string Value { get; set; } = string.Empty;

    /// <summary>參數說明</summary>
    [StringLength(500, ErrorMessage = "說明長度不可超過 500 字元")]
    [Display(Name = "說明")]
    public string? Description { get; set; }

    /// <summary>分組名稱</summary>
    [StringLength(50, ErrorMessage = "分組名稱長度不可超過 50 字元")]
    [Display(Name = "分組")]
    public string? Group { get; set; }
}
