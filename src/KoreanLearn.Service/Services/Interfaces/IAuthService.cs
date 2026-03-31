using System.Security.Claims;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>身份驗證業務邏輯介面（登入、註冊、登出、使用者資訊查詢）</summary>
public interface IAuthService
{
    /// <summary>使用帳號密碼登入</summary>
    Task<ServiceResult> LoginAsync(string email, string password, bool rememberMe, CancellationToken ct = default);

    /// <summary>註冊新帳號（不自動登入，需先驗證 Email），回傳 ServiceResult，Data 為 UserId</summary>
    Task<ServiceResult<string>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>登出目前使用者</summary>
    Task LogoutAsync(CancellationToken ct = default);

    /// <summary>檢查使用者是否已登入</summary>
    bool IsSignedIn(ClaimsPrincipal user);

    /// <summary>取得目前使用者名稱</summary>
    string? GetUserName(ClaimsPrincipal user);

    /// <summary>取得目前使用者 ID</summary>
    string? GetUserId(ClaimsPrincipal user);

    /// <summary>取得使用者所屬角色清單</summary>
    Task<IList<string>> GetRolesAsync(ClaimsPrincipal user);

    /// <summary>檢查使用者是否屬於指定角色</summary>
    bool IsInRole(ClaimsPrincipal user, string role);

    /// <summary>透過外部登入資訊尋找或建立使用者並完成登入</summary>
    Task<(bool Success, string? DisplayName, bool IsNewUser)> ExternalLoginAsync(
        string provider, string providerKey, string? email, string? displayName);

    /// <summary>取得已設定的外部登入提供者名稱清單</summary>
    Task<IEnumerable<string>> GetExternalAuthenticationSchemesAsync(CancellationToken ct = default);

    /// <summary>取得使用者個人資料供編輯</summary>
    Task<EditProfileRequest?> GetProfileAsync(string userId, CancellationToken ct = default);

    /// <summary>更新使用者個人資料</summary>
    Task<ServiceResult> UpdateProfileAsync(string userId, EditProfileRequest request, CancellationToken ct = default);

    /// <summary>產生密碼重設 Token 並回傳</summary>
    Task<(string? Token, string? Email)> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default);

    /// <summary>使用 Token 重設密碼</summary>
    Task<ServiceResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default);

    /// <summary>變更密碼（需提供舊密碼）</summary>
    Task<ServiceResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default);

    /// <summary>產生 Email 確認 Token（by UserId）</summary>
    Task<(string? Token, string? Email)> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken ct = default);

    /// <summary>以 Email 尋找使用者並產生 Email 確認 Token，回傳 (Token, UserId, Email)</summary>
    Task<(string? Token, string? UserId, string? Email)> GenerateEmailConfirmationTokenByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>確認 Email</summary>
    Task<ServiceResult> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default);

    /// <summary>檢查密碼是否重複使用（最近 5 次）</summary>
    Task<bool> IsPasswordReusedAsync(string userId, string newPassword, CancellationToken ct = default);

    /// <summary>儲存密碼到歷史紀錄</summary>
    Task SavePasswordHistoryAsync(string userId, string passwordHash, CancellationToken ct = default);

    // ─── 2FA 狀態管理 ─────────────────────────────────────

    /// <summary>取得使用者 2FA 詳細狀態（含偏好方式、是否已設定驗證器）</summary>
    Task<TwoFactorStatusInfo> GetTwoFactorStatusAsync(string userId, CancellationToken ct = default);

    /// <summary>停用 2FA 並重設驗證器金鑰</summary>
    Task<ServiceResult> DisableTwoFactorAsync(string userId, CancellationToken ct = default);

    // ─── TOTP 驗證器 App ────────────────────────────────

    /// <summary>初始化 TOTP 驗證器設定（重設金鑰並產生 QR Code URI）</summary>
    Task<AuthenticatorSetupInfo> SetupAuthenticatorAsync(string userId, CancellationToken ct = default);

    /// <summary>取得目前驗證器設定（不重設金鑰，用於 POST 驗證失敗重新顯示）</summary>
    Task<AuthenticatorSetupInfo> GetAuthenticatorSetupAsync(string userId, CancellationToken ct = default);

    /// <summary>驗證 TOTP 驗證碼並啟用驗證器 2FA</summary>
    Task<ServiceResult> EnableAuthenticatorAsync(string userId, string verificationCode, CancellationToken ct = default);

    // ─── Email OTP ──────────────────────────────────────

    /// <summary>產生 Email 2FA 設定驗證碼</summary>
    Task<(bool Success, string Message, string? Code)> GenerateEmailTwoFactorSetupCodeAsync(string userId, CancellationToken ct = default);

    /// <summary>驗證 Email 驗證碼並啟用 Email 2FA</summary>
    Task<ServiceResult> EnableEmailTwoFactorAsync(string userId, string verificationCode, CancellationToken ct = default);

    // ─── 2FA 登入流程 ───────────────────────────────────

    /// <summary>取得等待 2FA 驗證的使用者資訊</summary>
    Task<(string? UserId, string? PreferredMethod, string? Email)> GetTwoFactorUserInfoAsync(CancellationToken ct = default);

    /// <summary>使用 TOTP 驗證碼完成 2FA 登入</summary>
    Task<(bool Success, bool IsLockedOut, int LockoutMinutes)> TwoFactorAuthenticatorLoginAsync(string code, bool rememberMe);

    /// <summary>使用 Email 驗證碼完成 2FA 登入</summary>
    Task<(bool Success, bool IsLockedOut, int LockoutMinutes)> TwoFactorEmailLoginAsync(string code, bool rememberMe);

    /// <summary>在登入流程中產生 Email 2FA 驗證碼</summary>
    Task<(bool Success, string Message, string? Code)> GenerateTwoFactorEmailCodeAsync(CancellationToken ct = default);
}

/// <summary>2FA 狀態資訊 DTO</summary>
public class TwoFactorStatusInfo
{
    /// <summary>是否已啟用 2FA</summary>
    public bool IsEnabled { get; set; }

    /// <summary>偏好的驗證方式（Authenticator / Email）</summary>
    public string? PreferredMethod { get; set; }

    /// <summary>是否已設定 TOTP 驗證器</summary>
    public bool HasAuthenticator { get; set; }

    /// <summary>使用者 Email</summary>
    public string? Email { get; set; }
}

/// <summary>TOTP 驗證器設定資訊 DTO</summary>
public class AuthenticatorSetupInfo
{
    /// <summary>格式化後的手動輸入金鑰</summary>
    public string SharedKey { get; set; } = string.Empty;

    /// <summary>otpauth:// URI（用於產生 QR Code）</summary>
    public string AuthenticatorUri { get; set; } = string.Empty;
}

/// <summary>個人資料編輯 DTO</summary>
public class EditProfileRequest
{
    /// <summary>使用者顯示名稱</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>手機號碼（選填）</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>電子信箱（唯讀顯示用）</summary>
    public string? Email { get; set; }
}

/// <summary>註冊請求 DTO</summary>
public class RegisterRequest
{
    /// <summary>使用者顯示名稱</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>電子信箱（同時作為登入帳號）</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>手機號碼（選填）</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>密碼</summary>
    public string Password { get; set; } = string.Empty;
}

