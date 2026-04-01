using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Controllers;

/// <summary>訂閱方案 Controller，提供方案列表瀏覽與訂閱功能</summary>
public class SubscriptionController(
    ISubscriptionService subscriptionService,
    ILogger<SubscriptionController> logger) : BaseController
{
    /// <summary>訂閱方案列表頁，顯示所有啟用中的方案與使用者目前的訂閱狀態</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        logger.LogInformation("瀏覽訂閱方案列表 | UserId={UserId}", GetCurrentUserId());
        var plans = await subscriptionService.GetActivePlansAsync(ct);
        string? userId = GetCurrentUserId();
        ViewBag.CurrentSub = userId is not null
            ? await subscriptionService.GetUserSubscriptionAsync(userId, ct)
            : null;
        return View(plans);
    }

    /// <summary>訂閱確認結帳頁（GET），顯示方案明細與付款按鈕</summary>
    [Authorize]
    public async Task<IActionResult> Checkout(int planId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("進入訂閱結帳頁面 | PlanId={PlanId} | UserId={UserId}", planId, userId);

        // 檢查是否已有有效訂閱
        var existingSub = await subscriptionService.GetUserSubscriptionAsync(userId, ct);
        if (existingSub is not null)
        {
            logger.LogWarning("使用者已有有效訂閱，重複訂閱 | PlanId={PlanId} | UserId={UserId}", planId, userId);
            TempData[TempDataKeys.Warning] = "您已有有效訂閱，無需重複訂閱。";
            return RedirectToAction(nameof(Index));
        }

        var plans = await subscriptionService.GetActivePlansAsync(ct);
        var plan = plans.FirstOrDefault(p => p.Id == planId);
        if (plan is null)
        {
            logger.LogWarning("訂閱方案不存在或已停用 | PlanId={PlanId} | UserId={UserId}", planId, userId);
            TempData[TempDataKeys.Error] = "方案不存在或已停用";
            return RedirectToAction(nameof(Index));
        }

        return View(plan);
    }

    /// <summary>確認訂閱（POST），模擬付款並建立訂閱</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmSubscribe(int planId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("使用者訂閱方案 | PlanId={PlanId} | UserId={UserId}", planId, userId);
        var result = await subscriptionService.SubscribeAsync(userId, planId, ct);
        if (result.IsSuccess)
            logger.LogInformation("訂閱方案成功 | PlanId={PlanId} | UserId={UserId}", planId, userId);
        else
            logger.LogWarning("訂閱方案失敗 | PlanId={PlanId} | Error={Error} | UserId={UserId}",
                planId, result.ErrorMessage, userId);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "訂閱成功！所有課程已解鎖" : (result.ErrorMessage ?? "訂閱失敗");
        return RedirectToAction(nameof(Index));
    }
}
