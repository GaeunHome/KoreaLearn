using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Controllers;

/// <summary>訂閱方案 Controller，提供方案列表瀏覽與訂閱功能</summary>
public class SubscriptionController(ISubscriptionService subscriptionService) : Controller
{
    /// <summary>訂閱方案列表頁，顯示所有啟用中的方案與使用者目前的訂閱狀態</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var plans = await subscriptionService.GetActivePlansAsync(ct);
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.CurrentSub = userId is not null
            ? await subscriptionService.GetUserSubscriptionAsync(userId, ct)
            : null;
        return View(plans);
    }

    /// <summary>訂閱方案（POST），訂閱成功後解鎖所有課程並導回方案列表</summary>
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
