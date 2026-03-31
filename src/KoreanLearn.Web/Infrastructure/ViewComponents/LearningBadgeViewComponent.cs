using System.Security.Claims;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Infrastructure.ViewComponents;

/// <summary>學習提醒 Badge ViewComponent，在 Navbar 顯示待複習字卡數量</summary>
public class LearningBadgeViewComponent(IFlashcardLearnService flashcardService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
            return Content(string.Empty);

        var userId = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Content(string.Empty);

        var dueCount = await flashcardService.GetDueCardCountAsync(userId);
        return View(dueCount);
    }
}
