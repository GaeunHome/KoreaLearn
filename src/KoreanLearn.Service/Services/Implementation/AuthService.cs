using System.Security.Claims;
using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>身份驗證業務邏輯實作，封裝 ASP.NET Core Identity 的登入、註冊與使用者資訊查詢</summary>
public class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IUnitOfWork uow,
    ILogger<AuthService> logger) : IAuthService
{
    /// <inheritdoc />
    public async Task<ServiceResult> LoginAsync(
        string email, string password, bool rememberMe, CancellationToken ct = default)
    {
        // 先檢查 Email 是否已驗證
        var checkUser = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (checkUser is not null && !await userManager.IsEmailConfirmedAsync(checkUser).ConfigureAwait(false))
        {
            logger.LogWarning("登入嘗試失敗：Email 尚未驗證 | Email={Email}", email);
            return ServiceResult.Failure("EMAIL_NOT_CONFIRMED");
        }

        var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            logger.LogInformation("使用者登入成功 | Email={Email}", email);
            return ServiceResult.Success();
        }

        if (result.RequiresTwoFactor)
        {
            logger.LogInformation("2FA 驗證需要 | Email={Email}", email);
            return ServiceResult.Failure("2FA");
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("帳號被鎖定 | Email={Email}", email);
            return ServiceResult.Failure("帳號已被鎖定，請稍後再試。");
        }

        return ServiceResult.Failure("帳號或密碼錯誤。");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false, // 需先驗證 Email 才能登入
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            // 將 Identity 錯誤碼轉換為中文訊息
            var msg = string.Join("；", result.Errors.Select(e => e.Code switch
            {
                "DuplicateEmail" or "DuplicateUserName" => "此電子信箱已被註冊",
                "PasswordTooShort" => "密碼長度至少 6 碼",
                _ => e.Description
            }));
            return ServiceResult<string>.Failure(msg);
        }

        // 新註冊使用者預設為 Student 角色
        await userManager.AddToRoleAsync(user, "Student").ConfigureAwait(false);
        logger.LogInformation("新使用者註冊成功（待驗證 Email）| Email={Email}", request.Email);

        // 儲存初始密碼到歷史紀錄
        await SavePasswordHistoryAsync(user.Id, user.PasswordHash!, ct).ConfigureAwait(false);

        // 回傳 UserId，讓 Controller 產生驗證連結並寄送 Email
        return ServiceResult<string>.Success(user.Id);
    }

    /// <inheritdoc />
    public async Task LogoutAsync(CancellationToken ct = default)
    {
        await signInManager.SignOutAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool IsSignedIn(ClaimsPrincipal user)
        => signInManager.IsSignedIn(user);

    /// <inheritdoc />
    public string? GetUserName(ClaimsPrincipal user)
        => user.Identity?.Name;

    /// <inheritdoc />
    public string? GetUserId(ClaimsPrincipal user)
        => userManager.GetUserId(user);

    /// <inheritdoc />
    public async Task<IList<string>> GetRolesAsync(ClaimsPrincipal user)
    {
        var appUser = await userManager.GetUserAsync(user).ConfigureAwait(false);
        return appUser is null ? [] : await userManager.GetRolesAsync(appUser).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool IsInRole(ClaimsPrincipal user, string role)
        => user.IsInRole(role);

    /// <inheritdoc />
    public async Task<(bool Success, string? DisplayName, bool IsNewUser)> ExternalLoginAsync(
        string provider, string providerKey, string? email, string? displayName)
    {
        // 嘗試以既有的外部登入資訊登入
        var result = await signInManager.ExternalLoginSignInAsync(provider, providerKey, isPersistent: false)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            logger.LogInformation("外部登入成功 | Provider={Provider}", provider);
            return (true, null, false);
        }

        // 若已有相同 Email 的帳號，基於安全考量不自動連結
        if (!string.IsNullOrEmpty(email))
        {
            var existingUser = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (existingUser is not null)
            {
                logger.LogWarning("外部登入失敗：Email 已被其他帳號使用 | Provider={Provider}, Email={Email}", provider, email);
                return (false, null, false);
            }
        }

        // 建立新使用者
        var user = new AppUser
        {
            UserName = email ?? $"{provider}_{providerKey}",
            Email = email,
            DisplayName = displayName ?? "使用者",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            logger.LogError("外部登入建立使用者失敗 | Provider={Provider}, Errors={Errors}",
                provider, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return (false, null, false);
        }

        // 指派 Student 角色並新增外部登入連結
        await userManager.AddToRoleAsync(user, "Student").ConfigureAwait(false);
        await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider)).ConfigureAwait(false);

        // 登入新使用者
        await signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);
        logger.LogInformation("外部登入建立新使用者成功 | Provider={Provider}, Email={Email}", provider, email);

        return (true, user.DisplayName, true);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetExternalAuthenticationSchemesAsync(CancellationToken ct = default)
    {
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync().ConfigureAwait(false);
        return schemes.Select(s => s.Name);
    }

    /// <inheritdoc />
    public async Task<EditProfileRequest?> GetProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return null;
        return new EditProfileRequest
        {
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateProfileAsync(string userId, EditProfileRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        user.DisplayName = request.DisplayName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await userManager.UpdateAsync(user).ConfigureAwait(false);
        return result.Succeeded ? ServiceResult.Success() : ServiceResult.Failure("更新失敗");
    }

    /// <inheritdoc />
    public async Task<(string? Token, string? Email)> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is null) return (null, null);
        var token = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
        return (token, user.Email);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");
        var result = await userManager.ResetPasswordAsync(user, token, newPassword).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var msg = string.Join("；", result.Errors.Select(e => e.Code switch
            {
                "PasswordTooShort" => "密碼長度至少 6 碼",
                "InvalidToken" => "重設連結已過期，請重新申請",
                _ => e.Description
            }));
            return ServiceResult.Failure(msg);
        }
        // 儲存新密碼到歷史紀錄
        var updatedUser = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (updatedUser is not null)
            await SavePasswordHistoryAsync(updatedUser.Id, updatedUser.PasswordHash!, ct).ConfigureAwait(false);

        logger.LogInformation("密碼重設成功 | Email={Email}", email);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        // 檢查密碼歷史
        var isReused = await IsPasswordReusedAsync(userId, newPassword, ct).ConfigureAwait(false);
        if (isReused) return ServiceResult.Failure("此密碼最近已使用過，請設定不同的密碼");

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var msg = string.Join("；", result.Errors.Select(e => e.Code switch
            {
                "PasswordMismatch" => "目前密碼不正確",
                "PasswordTooShort" => "新密碼長度至少 6 碼",
                _ => e.Description
            }));
            return ServiceResult.Failure(msg);
        }

        // 儲存新密碼到歷史紀錄
        var updatedUser = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (updatedUser is not null)
            await SavePasswordHistoryAsync(userId, updatedUser.PasswordHash!, ct).ConfigureAwait(false);

        logger.LogInformation("密碼變更成功 | UserId={UserId}", userId);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<(string? Token, string? Email)> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return (null, null);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
        return (token, user.Email);
    }

    /// <inheritdoc />
    public async Task<(string? Token, string? UserId, string? Email)> GenerateEmailConfirmationTokenByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is null || await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
            return (null, null, null);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
        return (token, user.Id, user.Email);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        if (await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
            return ServiceResult.Success(); // 已驗證

        var result = await userManager.ConfirmEmailAsync(user, token).ConfigureAwait(false);
        if (!result.Succeeded)
            return ServiceResult.Failure("驗證連結已過期，請重新申請");

        // 驗證成功後自動登入
        await signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);
        logger.LogInformation("Email 驗證成功並自動登入 | UserId={UserId}", userId);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<bool> IsPasswordReusedAsync(string userId, string newPassword, CancellationToken ct = default)
    {
        var histories = await uow.PasswordHistories.GetByUserIdAsync(userId, 5, ct).ConfigureAwait(false);
        var hasher = userManager.PasswordHasher;
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return false;

        foreach (var history in histories)
        {
            var result = hasher.VerifyHashedPassword(user, history.PasswordHash, newPassword);
            if (result != PasswordVerificationResult.Failed)
                return true;
        }
        return false;
    }

    /// <inheritdoc />
    public async Task SavePasswordHistoryAsync(string userId, string passwordHash, CancellationToken ct = default)
    {
        await uow.PasswordHistories.AddAsync(new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        }, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    // ─── 2FA 狀態管理 ─────────────────────────────────────

    /// <inheritdoc />
    public async Task<TwoFactorStatusInfo> GetTwoFactorStatusAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return new TwoFactorStatusInfo();

        var hasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false) is not null;

        return new TwoFactorStatusInfo
        {
            IsEnabled = await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false),
            PreferredMethod = user.PreferredTwoFactorMethod.ToString(),
            HasAuthenticator = hasAuthenticator,
            Email = user.Email
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DisableTwoFactorAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        if (!await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false))
            return ServiceResult.Failure("兩步驟驗證尚未啟用");

        await userManager.SetTwoFactorEnabledAsync(user, false).ConfigureAwait(false);
        user.PreferredTwoFactorMethod = TwoFactorMethod.None;
        await userManager.UpdateAsync(user).ConfigureAwait(false);
        await userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(false);

        logger.LogInformation("2FA 已停用 | UserId={UserId}", userId);
        return ServiceResult.Success();
    }

    // ─── TOTP 驗證器 App ────────────────────────────────

    /// <inheritdoc />
    public async Task<AuthenticatorSetupInfo> SetupAuthenticatorAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false)
            ?? throw new InvalidOperationException("使用者不存在");

        await userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(false);
        return await BuildAuthenticatorInfoAsync(user).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<AuthenticatorSetupInfo> GetAuthenticatorSetupAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false)
            ?? throw new InvalidOperationException("使用者不存在");

        return await BuildAuthenticatorInfoAsync(user).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> EnableAuthenticatorAsync(string userId, string verificationCode, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        var code = verificationCode.Replace(" ", "").Replace("-", "");
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, code).ConfigureAwait(false);

        if (!isValid) return ServiceResult.Failure("驗證碼不正確，請重新輸入");

        await userManager.SetTwoFactorEnabledAsync(user, true).ConfigureAwait(false);
        user.PreferredTwoFactorMethod = TwoFactorMethod.Authenticator;
        await userManager.UpdateAsync(user).ConfigureAwait(false);

        logger.LogInformation("TOTP 驗證器啟用成功 | UserId={UserId}", userId);
        return ServiceResult.Success();
    }

    // ─── Email OTP ──────────────────────────────────────

    /// <inheritdoc />
    public async Task<(bool Success, string Message, string? Code)> GenerateEmailTwoFactorSetupCodeAsync(
        string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return (false, "使用者不存在", null);

        var code = await userManager.GenerateTwoFactorTokenAsync(user, "Email").ConfigureAwait(false);
        return (true, "驗證碼已產生", code);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> EnableEmailTwoFactorAsync(string userId, string verificationCode, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        var isValid = await userManager.VerifyTwoFactorTokenAsync(user, "Email", verificationCode).ConfigureAwait(false);
        if (!isValid) return ServiceResult.Failure("驗證碼不正確或已過期，請重新取得");

        await userManager.SetTwoFactorEnabledAsync(user, true).ConfigureAwait(false);
        user.PreferredTwoFactorMethod = TwoFactorMethod.Email;
        await userManager.UpdateAsync(user).ConfigureAwait(false);

        logger.LogInformation("Email 2FA 啟用成功 | UserId={UserId}", userId);
        return ServiceResult.Success();
    }

    // ─── 2FA 登入流程 ───────────────────────────────────

    /// <inheritdoc />
    public async Task<(string? UserId, string? PreferredMethod, string? Email)> GetTwoFactorUserInfoAsync(CancellationToken ct = default)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync().ConfigureAwait(false);
        if (user is null) return (null, null, null);
        return (user.Id, user.PreferredTwoFactorMethod.ToString(), user.Email);
    }

    /// <inheritdoc />
    public async Task<(bool Success, bool IsLockedOut, int LockoutMinutes)> TwoFactorAuthenticatorLoginAsync(
        string code, bool rememberMe)
    {
        var cleanCode = code.Replace(" ", "").Replace("-", "");
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(cleanCode, rememberMe, rememberClient: false)
            .ConfigureAwait(false);
        return HandleTwoFactorResult(result);
    }

    /// <inheritdoc />
    public async Task<(bool Success, bool IsLockedOut, int LockoutMinutes)> TwoFactorEmailLoginAsync(
        string code, bool rememberMe)
    {
        var result = await signInManager.TwoFactorSignInAsync("Email", code, rememberMe, rememberClient: false)
            .ConfigureAwait(false);
        return HandleTwoFactorResult(result);
    }

    /// <inheritdoc />
    public async Task<(bool Success, string Message, string? Code)> GenerateTwoFactorEmailCodeAsync(CancellationToken ct = default)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync().ConfigureAwait(false);
        if (user is null) return (false, "找不到待驗證使用者，請重新登入", null);

        var code = await userManager.GenerateTwoFactorTokenAsync(user, "Email").ConfigureAwait(false);
        return (true, "驗證碼已產生", code);
    }

    // ─── 私有輔助方法 ───────────────────────────────────

    private async Task<AuthenticatorSetupInfo> BuildAuthenticatorInfoAsync(AppUser user)
    {
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false)
            ?? throw new InvalidOperationException("無法取得驗證器金鑰");

        var email = await userManager.GetEmailAsync(user).ConfigureAwait(false) ?? "user";
        var authenticatorUri = $"otpauth://totp/KoreanLearn:{email}?secret={unformattedKey}&issuer=KoreanLearn&digits=6";

        return new AuthenticatorSetupInfo
        {
            SharedKey = FormatAuthenticatorKey(unformattedKey),
            AuthenticatorUri = authenticatorUri
        };
    }

    private static string FormatAuthenticatorKey(string unformattedKey)
    {
        var result = new System.Text.StringBuilder();
        var currentPosition = 0;

        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
            result.Append(unformattedKey.AsSpan(currentPosition));

        return result.ToString().ToUpperInvariant();
    }

    private (bool Success, bool IsLockedOut, int LockoutMinutes) HandleTwoFactorResult(SignInResult result)
    {
        if (result.Succeeded) return (true, false, 0);

        if (result.IsLockedOut)
        {
            var lockoutMinutes = (int)userManager.Options.Lockout.DefaultLockoutTimeSpan.TotalMinutes;
            return (false, true, lockoutMinutes);
        }

        return (false, false, 0);
    }
}
