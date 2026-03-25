using System.Security.Claims;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>身份驗證業務邏輯介面（登入、註冊、登出、使用者資訊查詢）</summary>
public interface IAuthService
{
    /// <summary>使用帳號密碼登入</summary>
    Task<ServiceResult> LoginAsync(string email, string password, bool rememberMe, CancellationToken ct = default);

    /// <summary>註冊新帳號並自動登入，預設角色為 Student</summary>
    Task<ServiceResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>登出目前使用者</summary>
    Task LogoutAsync();

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
