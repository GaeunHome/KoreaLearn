using System.ComponentModel.DataAnnotations;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace KoreanLearn.Web.Areas.Identity.Pages.Account;

[EnableRateLimiting("auth")]
public class RegisterModel(IAuthService authService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "請輸入顯示名稱")]
        [StringLength(50, ErrorMessage = "名稱不得超過 50 字")]
        [Display(Name = "顯示名稱")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入電子信箱")]
        [EmailAddress(ErrorMessage = "電子信箱格式不正確")]
        [Display(Name = "電子信箱")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "電話格式不正確")]
        [StringLength(20)]
        [Display(Name = "聯絡電話")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(100, ErrorMessage = "密碼長度至少 {2} 碼", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請再次輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "兩次輸入的密碼不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "我同意服務條款與隱私權政策")]
        public bool AgreeTerms { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!Input.AgreeTerms)
            ModelState.AddModelError("Input.AgreeTerms", "請同意服務條款");

        if (!ModelState.IsValid)
            return Page();

        var result = await authService.RegisterAsync(new RegisterRequest
        {
            DisplayName = Input.DisplayName,
            Email = Input.Email,
            PhoneNumber = Input.PhoneNumber,
            Password = Input.Password
        });

        if (result.IsSuccess)
        {
            TempData["Success"] = "註冊成功，歡迎加入 KoreanLearn！";
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "註冊失敗");
        return Page();
    }
}
