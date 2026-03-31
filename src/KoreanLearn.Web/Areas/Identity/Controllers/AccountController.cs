using System.Security.Claims;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Identity;
using KoreanLearn.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KoreanLearn.Web.Areas.Identity.Controllers;

/// <summary>帳號相關 Controller，處理登入、註冊、登出、個人資料管理等功能</summary>
public class AccountController(IAuthService authService, IEmailService emailService) : IdentityBaseController
{
    // ─── Login ──────────────────────────────────────────────

    /// <summary>顯示登入頁面</summary>
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");
        ViewBag.ExternalProviders = (await authService.GetExternalAuthenticationSchemesAsync()).ToList();
        return View(new LoginViewModel());
    }

    /// <summary>處理登入表單提交，驗證成功後導向 ReturnUrl</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ExternalProviders = (await authService.GetExternalAuthenticationSchemesAsync()).ToList();
            return View(model);
        }

        var result = await authService.LoginAsync(model.Email, model.Password, model.RememberMe);

        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "登入成功，歡迎回來！";
            return LocalRedirect(returnUrl);
        }

        // 2FA 需要驗證碼，導向兩步驟驗證頁面
        if (result.ErrorMessage == "2FA")
        {
            return RedirectToAction(nameof(LoginTwoFactor), new { rememberMe = model.RememberMe, returnUrl });
        }

        // Email 尚未驗證，導向提示頁面
        if (result.ErrorMessage == "EMAIL_NOT_CONFIRMED")
        {
            TempData[TempDataKeys.Warning] = "您的 Email 尚未驗證，請先點擊驗證信中的連結。";
            return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email });
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "登入失敗");
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.ExternalProviders = (await authService.GetExternalAuthenticationSchemesAsync()).ToList();
        return View(model);
    }

    // ─── Register ───────────────────────────────────────────

    /// <summary>顯示註冊頁面</summary>
    [EnableRateLimiting("auth")]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");
        return View(new RegisterViewModel());
    }

    /// <summary>處理註冊表單提交，建立帳號成功後自動登入並導向 ReturnUrl</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!model.AgreeTerms)
            ModelState.AddModelError(nameof(model.AgreeTerms), "請同意服務條款");

        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var result = await authService.RegisterAsync(new RegisterRequest
        {
            DisplayName = model.DisplayName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Password = model.Password
        });

        if (result.IsSuccess)
        {
            // 產生 Email 確認 Token 並寄送驗證信
            var (token, _) = await authService.GenerateEmailConfirmationTokenAsync(result.Data!);
            if (token is not null)
            {
                await SendConfirmationEmailAsync(result.Data!, model.Email, token);
            }
            return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email });
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "註冊失敗");
        ViewBag.ReturnUrl = returnUrl;
        return View(model);
    }

    // ─── Logout ─────────────────────────────────────────────

    /// <summary>處理登出請求（POST），登出後導向首頁或指定 ReturnUrl</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await authService.LogoutAsync();
        TempData[TempDataKeys.Success] = "您已成功登出。";
        return returnUrl is not null ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home", new { area = "" });
    }

    // ─── External Login ─────────────────────────────────────

    /// <summary>發起外部登入挑戰，導向第三方登入頁面</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { area = "Identity", returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    /// <summary>處理外部登入回呼，建立或連結使用者帳號</summary>
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (info?.Principal is null)
        {
            TempData[TempDataKeys.Error] = "外部登入驗證失敗";
            return RedirectToAction(nameof(Login));
        }

        var provider = info.Properties?.Items[".AuthScheme"] ?? "Unknown";
        var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var displayName = info.Principal.FindFirstValue(ClaimTypes.Name);

        // 清除外部登入 cookie
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var (success, name, isNew) = await authService.ExternalLoginAsync(provider, providerKey, email, displayName);
        if (success)
        {
            TempData[TempDataKeys.Success] = isNew ? $"歡迎加入 KoreanLearn，{name}！" : "登入成功，歡迎回來！";
            return LocalRedirect(returnUrl);
        }

        TempData[TempDataKeys.Error] = "外部登入失敗，該 Email 可能已被其他帳號使用。";
        return RedirectToAction(nameof(Login));
    }

    // ─── Profile ────────────────────────────────────────────

    /// <summary>顯示個人資料頁面</summary>
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return RedirectToAction(nameof(Login));

        var profile = await authService.GetProfileAsync(userId);
        if (profile is null) return NotFound();

        var model = new ProfileViewModel
        {
            DisplayName = profile.DisplayName,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email
        };
        return View(model);
    }

    /// <summary>處理個人資料更新表單</summary>
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetAuthorizedUserId();
        var result = await authService.UpdateProfileAsync(userId, new EditProfileRequest
        {
            DisplayName = model.DisplayName,
            PhoneNumber = model.PhoneNumber
        });

        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "個人資料已更新";
            return RedirectToAction(nameof(Profile));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "更新失敗");
        return View(model);
    }

    // ─── Change Password ────────────────────────────────────

    /// <summary>顯示變更密碼頁面</summary>
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    /// <summary>處理變更密碼表單</summary>
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetAuthorizedUserId();
        var result = await authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "密碼已成功變更";
            return RedirectToAction(nameof(Profile));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "變更失敗");
        return View(model);
    }

    // ─── Forgot Password ────────────────────────────────────

    /// <summary>顯示忘記密碼頁面</summary>
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    /// <summary>處理忘記密碼表單，產生密碼重設 Token 並寄送 Email</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (token, email) = await authService.GeneratePasswordResetTokenAsync(model.Email);

        // 有 Token 表示使用者存在，寄送重設信
        if (token is not null && email is not null)
        {
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account",
                new { area = "Identity", email, token }, Request.Scheme);

            var html = $"""
                <div style="font-family: 'Noto Sans TC', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                    <div style="text-align: center; padding: 20px 0;">
                        <h2 style="color: #2B3A67;">KoreanLearn 韓文學習平台</h2>
                    </div>
                    <div style="background: #f8f6f3; border-radius: 8px; padding: 30px;">
                        <h3 style="color: #1E2A4A;">密碼重設</h3>
                        <p>我們收到了您的密碼重設請求，請點擊下方按鈕設定新密碼：</p>
                        <div style="text-align: center; margin: 30px 0;">
                            <a href="{callbackUrl}"
                               style="background: #2B3A67; color: #fff; padding: 12px 30px; border-radius: 8px;
                                      text-decoration: none; font-weight: 600; display: inline-block;">
                                重設密碼
                            </a>
                        </div>
                        <p style="color: #7A7A7A; font-size: 0.85rem;">若您未要求重設密碼，請忽略此信。</p>
                    </div>
                </div>
                """;

            try
            {
                await emailService.SendEmailAsync(email, "KoreanLearn — 密碼重設", html);
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AccountController>>();
                logger.LogError(ex, "寄送密碼重設信失敗 | Email={Email}", email);
            }
        }

        // 無論是否找到使用者，都顯示相同訊息（防止帳號列舉）
        model.ShowConfirmation = true;
        model.ResetToken = token;
        model.ResetEmail = email ?? model.Email;
        return View(model);
    }

    // ─── Reset Password ─────────────────────────────────────

    /// <summary>顯示重設密碼頁面</summary>
    public IActionResult ResetPassword(string email, string token)
    {
        var model = new ResetPasswordViewModel { Email = email, Token = token };
        return View(model);
    }

    /// <summary>處理重設密碼表單</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await authService.ResetPasswordAsync(model.Email, model.Token, model.Password);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "密碼已重設，請使用新密碼登入";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "重設失敗");
        return View(model);
    }

    // ─── Register Confirmation ────────────────────────────────

    /// <summary>顯示註冊確認頁面，提示使用者前往信箱驗證</summary>
    public IActionResult RegisterConfirmation(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    /// <summary>重新寄送驗證信</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ResendConfirmationEmail(string email)
    {
        var (token, userId, _) = await authService.GenerateEmailConfirmationTokenByEmailAsync(email);
        if (token is not null && userId is not null)
        {
            await SendConfirmationEmailAsync(userId, email, token);
        }

        // 不透露帳號是否存在
        TempData[TempDataKeys.Success] = "若此信箱已註冊，驗證信將會寄出。";
        return RedirectToAction(nameof(RegisterConfirmation), new { email });
    }

    // ─── Confirm Email ──────────────────────────────────────

    /// <summary>處理 Email 確認連結，驗證成功後自動登入</summary>
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            ViewBag.ErrorMessage = "無效的確認連結";
            ViewBag.Confirmed = false;
            return View();
        }

        var result = await authService.ConfirmEmailAsync(userId, token);
        ViewBag.Confirmed = result.IsSuccess;
        ViewBag.ErrorMessage = result.IsSuccess ? null : result.ErrorMessage;

        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "Email 驗證成功，已自動登入，歡迎加入 KoreanLearn！";
        }

        return View();
    }

    // ─── Login Two Factor ───────────────────────────────────

    /// <summary>顯示 2FA 驗證碼輸入頁面（密碼驗證通過後導向此處）</summary>
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginTwoFactor(bool rememberMe, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");

        var (userId, preferredMethod, email) = await authService.GetTwoFactorUserInfoAsync();
        if (userId is null)
        {
            TempData[TempDataKeys.Error] = "驗證階段逾時，請重新登入";
            return RedirectToAction(nameof(Login));
        }

        // Email 方式自動產生驗證碼並寄送
        if (preferredMethod == "Email")
        {
            var (success, _, code) = await authService.GenerateTwoFactorEmailCodeAsync();
            if (success && code is not null && email is not null)
            {
                await Send2faCodeEmailAsync(email, code);
                ViewBag.DevCode = code; // 開發模式仍顯示於頁面
            }
        }

        var model = new LoginTwoFactorViewModel
        {
            RememberMe = rememberMe,
            Method = preferredMethod ?? "Authenticator"
        };

        ViewBag.MaskedEmail = email is not null ? MaskEmail(email) : null;
        return View(model);
    }

    /// <summary>驗證 2FA 驗證碼並完成登入</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginTwoFactor(LoginTwoFactorViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid) return View(model);

        var (success, isLockedOut, lockoutMinutes) = model.Method == "Email"
            ? await authService.TwoFactorEmailLoginAsync(model.Code, model.RememberMe)
            : await authService.TwoFactorAuthenticatorLoginAsync(model.Code, model.RememberMe);

        if (success)
        {
            TempData[TempDataKeys.Success] = "登入成功，歡迎回來！";
            return LocalRedirect(ViewBag.ReturnUrl as string ?? Url.Content("~/"));
        }

        if (isLockedOut)
        {
            TempData[TempDataKeys.Error] = $"帳號已被鎖定，請 {lockoutMinutes} 分鐘後再試。";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, "驗證碼不正確，請重新輸入");
        return View(model);
    }

    /// <summary>重新寄送 Email 2FA 驗證碼</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ResendTwoFactorCode(bool rememberMe, string? returnUrl = null)
    {
        var (_, _, email) = await authService.GetTwoFactorUserInfoAsync();
        if (email is null)
        {
            TempData[TempDataKeys.Error] = "驗證階段逾時，請重新登入";
            return RedirectToAction(nameof(Login));
        }

        var (success, _, code) = await authService.GenerateTwoFactorEmailCodeAsync();
        if (success && code is not null)
        {
            await Send2faCodeEmailAsync(email, code);
            TempData[TempDataKeys.Success] = "驗證碼已重新寄送";
        }

        return RedirectToAction(nameof(LoginTwoFactor), new { rememberMe, returnUrl });
    }

    // ─── Two Factor Authentication Management ───────────────

    /// <summary>2FA 管理總覽頁面</summary>
    [Authorize]
    public async Task<IActionResult> TwoFactorAuthentication()
    {
        var userId = GetAuthorizedUserId();
        var status = await authService.GetTwoFactorStatusAsync(userId);
        return View(status);
    }

    /// <summary>設定 TOTP 驗證器（GET：重設金鑰並顯示 QR Code）</summary>
    [Authorize]
    public async Task<IActionResult> SetupAuthenticator()
    {
        var userId = GetAuthorizedUserId();
        var info = await authService.SetupAuthenticatorAsync(userId);
        var model = new SetupAuthenticatorViewModel
        {
            SharedKey = info.SharedKey,
            AuthenticatorUri = info.AuthenticatorUri,
            QrCodeBase64 = GenerateQrCodeBase64(info.AuthenticatorUri)
        };
        return View(model);
    }

    /// <summary>驗證並啟用 TOTP 驗證器</summary>
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> SetupAuthenticator(SetupAuthenticatorViewModel model)
    {
        var userId = GetAuthorizedUserId();

        if (!ModelState.IsValid)
        {
            var existing = await authService.GetAuthenticatorSetupAsync(userId);
            model.SharedKey = existing.SharedKey;
            model.QrCodeBase64 = GenerateQrCodeBase64(existing.AuthenticatorUri);
            return View(model);
        }

        var result = await authService.EnableAuthenticatorAsync(userId, model.VerificationCode);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "TOTP 驗證器已啟用，下次登入時需要輸入驗證碼";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        var current = await authService.GetAuthenticatorSetupAsync(userId);
        model.SharedKey = current.SharedKey;
        model.QrCodeBase64 = GenerateQrCodeBase64(current.AuthenticatorUri);
        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "驗證失敗");
        return View(model);
    }

    /// <summary>設定 Email 2FA（GET：產生並寄送驗證碼）</summary>
    [Authorize, EnableRateLimiting("auth")]
    public async Task<IActionResult> SetupEmailTwoFactor()
    {
        var userId = GetAuthorizedUserId();
        var (success, message, code) = await authService.GenerateEmailTwoFactorSetupCodeAsync(userId);

        if (!success)
        {
            TempData[TempDataKeys.Error] = message;
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        var status = await authService.GetTwoFactorStatusAsync(userId);
        ViewBag.Email = status.Email;
        ViewBag.DevCode = code; // 開發模式仍顯示於頁面

        // 透過 Email 寄送驗證碼
        if (code is not null && status.Email is not null)
        {
            await Send2faCodeEmailAsync(status.Email, code);
        }

        return View();
    }

    /// <summary>驗證 Email 驗證碼並啟用 Email 2FA</summary>
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> SetupEmailTwoFactor(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(string.Empty, "請輸入驗證碼");
            return View();
        }

        var userId = GetAuthorizedUserId();
        var result = await authService.EnableEmailTwoFactorAsync(userId, code);

        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "Email 兩步驟驗證已啟用";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "驗證失敗");
        return View();
    }

    /// <summary>停用 2FA</summary>
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> DisableTwoFactor()
    {
        var userId = GetAuthorizedUserId();
        var result = await authService.DisableTwoFactorAsync(userId);

        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "兩步驟驗證已停用" : (result.ErrorMessage ?? "停用失敗");

        return RedirectToAction(nameof(TwoFactorAuthentication));
    }

    // ─── 私有輔助方法 ───────────────────────────────────────

    /// <summary>寄送 Email 驗證信</summary>
    private async Task SendConfirmationEmailAsync(string userId, string email, string token)
    {
        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
            new { area = "Identity", userId, token }, Request.Scheme);

        var html = $"""
            <div style="font-family: 'Noto Sans TC', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <div style="text-align: center; padding: 20px 0;">
                    <h2 style="color: #2B3A67;">KoreanLearn 韓文學習平台</h2>
                </div>
                <div style="background: #f8f6f3; border-radius: 8px; padding: 30px;">
                    <h3 style="color: #1E2A4A;">歡迎加入 KoreanLearn！</h3>
                    <p>請點擊下方按鈕驗證您的電子信箱：</p>
                    <div style="text-align: center; margin: 30px 0;">
                        <a href="{callbackUrl}"
                           style="background: #2B3A67; color: #fff; padding: 12px 30px; border-radius: 8px;
                                  text-decoration: none; font-weight: 600; display: inline-block;">
                            驗證 Email
                        </a>
                    </div>
                    <p style="color: #7A7A7A; font-size: 0.85rem;">
                        若按鈕無法點擊，請複製以下連結至瀏覽器：<br />
                        <a href="{callbackUrl}" style="color: #2B3A67; word-break: break-all;">{callbackUrl}</a>
                    </p>
                </div>
                <p style="text-align: center; color: #7A7A7A; font-size: 0.8rem; margin-top: 20px;">
                    此信件由系統自動發送，請勿直接回覆。
                </p>
            </div>
            """;

        try
        {
            await emailService.SendEmailAsync(email, "KoreanLearn — 驗證您的電子信箱", html);
        }
        catch (Exception ex)
        {
            // 寄送失敗不阻擋註冊流程，但記錄錯誤
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AccountController>>();
            logger.LogError(ex, "寄送驗證信失敗 | Email={Email}", email);
        }
    }

    /// <summary>寄送 2FA 驗證碼 Email</summary>
    private async Task Send2faCodeEmailAsync(string email, string code)
    {
        var html = $"""
            <div style="font-family: 'Noto Sans TC', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <div style="text-align: center; padding: 20px 0;">
                    <h2 style="color: #2B3A67;">KoreanLearn 韓文學習平台</h2>
                </div>
                <div style="background: #f8f6f3; border-radius: 8px; padding: 30px; text-align: center;">
                    <h3 style="color: #1E2A4A;">兩步驟驗證碼</h3>
                    <p>您的登入驗證碼為：</p>
                    <div style="font-size: 2rem; font-weight: 700; letter-spacing: 0.5em; color: #2B3A67;
                                background: #fff; border-radius: 8px; padding: 15px; margin: 20px 0;
                                border: 2px solid #E5E2DD;">
                        {code}
                    </div>
                    <p style="color: #7A7A7A; font-size: 0.85rem;">
                        此驗證碼有效期限為 5 分鐘。若非本人操作，請忽略此信。
                    </p>
                </div>
            </div>
            """;

        try
        {
            await emailService.SendEmailAsync(email, "KoreanLearn — 登入驗證碼", html);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AccountController>>();
            logger.LogError(ex, "寄送 2FA 驗證碼 Email 失敗 | Email={Email}", email);
        }
    }

    /// <summary>使用 QRCoder 產生 QR Code Base64 PNG 圖片</summary>
    private static string GenerateQrCodeBase64(string text)
    {
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
        var pngBytes = qrCode.GetGraphic(5);
        return Convert.ToBase64String(pngBytes);
    }

    /// <summary>遮罩 Email（例：te***@gmail.com）</summary>
    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2 || parts[0].Length <= 2) return email;
        var name = parts[0];
        var masked = name[..2] + new string('*', Math.Min(name.Length - 2, 5));
        return $"{masked}@{parts[1]}";
    }
}
