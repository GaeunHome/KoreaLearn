using System.ComponentModel.DataAnnotations;

namespace KoreanLearn.Service.ViewModels.Identity;

/// <summary>登入表單 ViewModel</summary>
public class LoginViewModel
{
    /// <summary>電子信箱</summary>
    [Required(ErrorMessage = "請輸入電子信箱")]
    [EmailAddress(ErrorMessage = "電子信箱格式不正確")]
    [Display(Name = "電子信箱")]
    public string Email { get; set; } = string.Empty;

    /// <summary>密碼</summary>
    [Required(ErrorMessage = "請輸入密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    /// <summary>是否記住登入狀態</summary>
    [Display(Name = "記住我")]
    public bool RememberMe { get; set; }
}

/// <summary>註冊表單 ViewModel</summary>
public class RegisterViewModel
{
    /// <summary>使用者顯示名稱</summary>
    [Required(ErrorMessage = "請輸入顯示名稱")]
    [StringLength(50, ErrorMessage = "名稱不得超過 50 字")]
    [Display(Name = "顯示名稱")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>電子信箱</summary>
    [Required(ErrorMessage = "請輸入電子信箱")]
    [EmailAddress(ErrorMessage = "電子信箱格式不正確")]
    [Display(Name = "電子信箱")]
    public string Email { get; set; } = string.Empty;

    /// <summary>聯絡電話（選填）</summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    [StringLength(20)]
    [Display(Name = "聯絡電話")]
    public string? PhoneNumber { get; set; }

    /// <summary>密碼</summary>
    [Required(ErrorMessage = "請輸入密碼")]
    [StringLength(100, ErrorMessage = "密碼長度至少 {2} 碼", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    /// <summary>確認密碼</summary>
    [Required(ErrorMessage = "請再次輸入密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "確認密碼")]
    [Compare("Password", ErrorMessage = "兩次輸入的密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>是否同意服務條款</summary>
    [Display(Name = "我同意服務條款與隱私權政策")]
    public bool AgreeTerms { get; set; }
}

/// <summary>個人資料 ViewModel</summary>
public class ProfileViewModel
{
    /// <summary>使用者顯示名稱</summary>
    [Required(ErrorMessage = "請輸入顯示名稱")]
    [StringLength(50, ErrorMessage = "名稱不得超過 50 字")]
    [Display(Name = "顯示名稱")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>聯絡電話（選填）</summary>
    [Phone(ErrorMessage = "電話格式不正確")]
    [StringLength(20)]
    [Display(Name = "聯絡電話")]
    public string? PhoneNumber { get; set; }

    /// <summary>電子信箱（唯讀）</summary>
    [Display(Name = "電子信箱")]
    public string? Email { get; set; }

    /// <summary>是否設有密碼（外部登入帳號為 false，不顯示「變更密碼」入口）</summary>
    public bool HasPassword { get; set; }
}

/// <summary>變更密碼 ViewModel</summary>
public class ChangePasswordViewModel
{
    /// <summary>目前密碼</summary>
    [Required(ErrorMessage = "請輸入目前密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "目前密碼")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>新密碼</summary>
    [Required(ErrorMessage = "請輸入新密碼")]
    [StringLength(100, ErrorMessage = "密碼長度至少 {2} 碼", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>確認新密碼</summary>
    [Required(ErrorMessage = "請再次輸入新密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "確認新密碼")]
    [Compare("NewPassword", ErrorMessage = "兩次輸入的密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>忘記密碼 ViewModel</summary>
public class ForgotPasswordViewModel
{
    /// <summary>電子信箱</summary>
    [Required(ErrorMessage = "請輸入電子信箱")]
    [EmailAddress(ErrorMessage = "電子信箱格式不正確")]
    [Display(Name = "電子信箱")]
    public string Email { get; set; } = string.Empty;

    /// <summary>是否顯示確認訊息</summary>
    public bool ShowConfirmation { get; set; }

    /// <summary>開發模式下顯示的重設 Token</summary>
    public string? ResetToken { get; set; }

    /// <summary>重設目標 Email</summary>
    public string? ResetEmail { get; set; }
}

/// <summary>重設密碼 ViewModel</summary>
public class ResetPasswordViewModel
{
    /// <summary>電子信箱</summary>
    [Required]
    public string Email { get; set; } = string.Empty;

    /// <summary>重設 Token</summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>新密碼</summary>
    [Required(ErrorMessage = "請輸入新密碼")]
    [StringLength(100, ErrorMessage = "密碼長度至少 {2} 碼", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string Password { get; set; } = string.Empty;

    /// <summary>確認新密碼</summary>
    [Required(ErrorMessage = "請再次輸入新密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "確認新密碼")]
    [Compare("Password", ErrorMessage = "兩次輸入的密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>2FA 驗證碼輸入 ViewModel</summary>
public class LoginTwoFactorViewModel
{
    /// <summary>驗證碼</summary>
    [Required(ErrorMessage = "請輸入驗證碼")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "驗證碼為 6 碼")]
    [Display(Name = "驗證碼")]
    public string Code { get; set; } = string.Empty;

    /// <summary>是否記住登入狀態</summary>
    public bool RememberMe { get; set; }

    /// <summary>驗證方式（Authenticator / Email）</summary>
    public string Method { get; set; } = string.Empty;
}

/// <summary>TOTP 驗證器設定 ViewModel</summary>
public class SetupAuthenticatorViewModel
{
    /// <summary>手動輸入金鑰（Base32 編碼）</summary>
    public string SharedKey { get; set; } = string.Empty;

    /// <summary>otpauth:// URI（用於 QR Code）</summary>
    public string AuthenticatorUri { get; set; } = string.Empty;

    /// <summary>QR Code 圖片（Base64 編碼 PNG）</summary>
    public string QrCodeBase64 { get; set; } = string.Empty;

    /// <summary>使用者輸入的驗證碼</summary>
    [Required(ErrorMessage = "請輸入驗證碼")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "驗證碼為 6 碼")]
    [Display(Name = "驗證碼")]
    public string VerificationCode { get; set; } = string.Empty;
}
