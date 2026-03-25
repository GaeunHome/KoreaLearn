using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Controllers;

public class SubscriptionController(ISubscriptionService subscriptionService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var plans = await subscriptionService.GetActivePlansAsync(ct);
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.CurrentSub = userId is not null
            ? await subscriptionService.GetUserSubscriptionAsync(userId, ct)
            : null;
        return View(plans);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(int planId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await subscriptionService.SubscribeAsync(userId, planId, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] =
            result.IsSuccess ? "訂閱成功！所有課程已解鎖" : (result.ErrorMessage ?? "訂閱失敗");
        return RedirectToAction(nameof(Index));
    }
}
