using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KoreanLearn.Web.Areas.Identity.Pages.Account;

public class LogoutModel(IAuthService authService) : PageModel
{
    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        await authService.LogoutAsync();
        TempData["Success"] = "您已成功登出。";
        return returnUrl is not null ? LocalRedirect(returnUrl) : RedirectToPage();
    }
}
