using System.ComponentModel.DataAnnotations;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace KoreanLearn.Web.Areas.Identity.Pages.Account;

/// <summary>登入頁面 PageModel，處理使用者帳號密碼登入（含速率限制）</summary>
[EnableRateLimiting("auth")]
public class LoginModel(IAuthService authService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    /// <summary>登入表單輸入模型</summary>
    public class InputModel
    {
        [Required(ErrorMessage = "請輸入電子信箱")]
        [EmailAddress(ErrorMessage = "電子信箱格式不正確")]
        [Display(Name = "電子信箱")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }
    }

    /// <summary>載入登入頁面，設定 ReturnUrl</summary>
    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    /// <summary>處理登入表單提交，驗證成功後導向 ReturnUrl</summary>
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
            return Page();

        var result = await authService.LoginAsync(Input.Email, Input.Password, Input.RememberMe);

        if (result.IsSuccess)
        {
            TempData["Success"] = "登入成功，歡迎回來！";
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "登入失敗");
        return Page();
    }
}
