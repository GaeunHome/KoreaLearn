using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KoreanLearn.Web.Areas.Identity.Pages.Account;

/// <summary>登出頁面 PageModel，處理使用者登出並清除驗證 Cookie</summary>
public class LogoutModel(IAuthService authService) : PageModel
{
    /// <summary>處理登出請求（POST），登出後導向首頁或指定 ReturnUrl</summary>
    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        await authService.LogoutAsync();
        TempData["Success"] = "您已成功登出。";
        return returnUrl is not null ? LocalRedirect(returnUrl) : RedirectToPage();
    }
}
