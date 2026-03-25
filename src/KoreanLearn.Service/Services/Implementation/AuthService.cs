using System.Security.Claims;
using KoreanLearn.Data.Entities;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<ServiceResult> LoginAsync(
        string email, string password, bool rememberMe, CancellationToken ct = default)
    {
        var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            logger.LogInformation("使用者登入成功 | Email={Email}", email);
            return ServiceResult.Success();
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("帳號被鎖定 | Email={Email}", email);
            return ServiceResult.Failure("帳號已被鎖定，請稍後再試。");
        }

        return ServiceResult.Failure("帳號或密碼錯誤。");
    }

    public async Task<ServiceResult> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var msg = string.Join("；", result.Errors.Select(e => e.Code switch
            {
                "DuplicateEmail" or "DuplicateUserName" => "此電子信箱已被註冊",
                "PasswordTooShort" => "密碼長度至少 6 碼",
                _ => e.Description
            }));
            return ServiceResult.Failure(msg);
        }

        await userManager.AddToRoleAsync(user, "Student").ConfigureAwait(false);
        logger.LogInformation("新使用者註冊成功 | Email={Email}", request.Email);
        await signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);

        return ServiceResult.Success();
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync().ConfigureAwait(false);
    }

    public bool IsSignedIn(ClaimsPrincipal user)
        => signInManager.IsSignedIn(user);

    public string? GetUserName(ClaimsPrincipal user)
        => user.Identity?.Name;

    public string? GetUserId(ClaimsPrincipal user)
        => userManager.GetUserId(user);

    public async Task<IList<string>> GetRolesAsync(ClaimsPrincipal user)
    {
        var appUser = await userManager.GetUserAsync(user).ConfigureAwait(false);
        return appUser is null ? [] : await userManager.GetRolesAsync(appUser).ConfigureAwait(false);
    }

    public bool IsInRole(ClaimsPrincipal user, string role)
        => user.IsInRole(role);
}
