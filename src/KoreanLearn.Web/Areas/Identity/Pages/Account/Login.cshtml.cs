using System.ComponentModel.DataAnnotations;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace KoreanLearn.Web.Areas.Identity.Pages.Account;

[EnableRateLimiting("auth")]
public class LoginModel(IAuthService authService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

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

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

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
