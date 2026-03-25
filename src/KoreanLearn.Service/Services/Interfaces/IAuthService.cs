using System.Security.Claims;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult> LoginAsync(string email, string password, bool rememberMe, CancellationToken ct = default);
    Task<ServiceResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task LogoutAsync();
    bool IsSignedIn(ClaimsPrincipal user);
    string? GetUserName(ClaimsPrincipal user);
    string? GetUserId(ClaimsPrincipal user);
    Task<IList<string>> GetRolesAsync(ClaimsPrincipal user);
    bool IsInRole(ClaimsPrincipal user, string role);
}

public class RegisterRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
}
