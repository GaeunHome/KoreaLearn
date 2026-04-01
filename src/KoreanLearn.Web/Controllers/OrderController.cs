using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Controllers;

/// <summary>訂單 Controller，處理使用者的訂單列表、建立訂單、結帳與模擬付款流程</summary>
[Authorize]
public class OrderController(
    IOrderService orderService,
    ILogger<OrderController> logger) : BaseController
{
    /// <summary>我的訂單列表（分頁），顯示當前使用者的所有訂單</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("瀏覽訂單列表 | Page={Page} | UserId={UserId}", page, userId);
        var result = await orderService.GetUserOrdersAsync(userId, page, DisplayConstants.OrderPageSize, ct);
        return View(result);
    }

    /// <summary>建立訂單（POST），成功後導向結帳頁；失敗導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("使用者建立訂單 | CourseId={CourseId} | UserId={UserId}", courseId, userId);
        var result = await orderService.CreateOrderAsync(userId, courseId, ct);
        if (result is { IsSuccess: true, Data: var orderId })
        {
            logger.LogInformation("建立訂單成功 | OrderId={OrderId} | CourseId={CourseId} | UserId={UserId}",
                orderId, courseId, userId);
            return RedirectToAction(nameof(Checkout), new { id = orderId });
        }

        logger.LogWarning("建立訂單失敗 | CourseId={CourseId} | Error={Error} | UserId={UserId}",
            courseId, result.ErrorMessage, userId);
        TempData[TempDataKeys.Error] = result.ErrorMessage ?? "建立訂單失敗";
        return RedirectToAction("Detail", "Course", new { id = courseId });
    }

    /// <summary>結帳頁面，顯示訂單明細與付款按鈕；找不到訂單時回傳 404</summary>
    public async Task<IActionResult> Checkout(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("進入結帳頁面 | OrderId={OrderId} | UserId={UserId}", id, userId);
        var order = await orderService.GetOrderDetailAsync(id, userId, ct);
        if (order is null)
        {
            logger.LogWarning("結帳頁面找不到訂單 | OrderId={OrderId} | UserId={UserId}", id, userId);
            return NotFound();
        }
        return View(order);
    }

    /// <summary>模擬付款（POST），成功後解鎖課程並導向訂單詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("使用者模擬付款 | OrderId={OrderId} | UserId={UserId}", id, userId);
        var result = await orderService.SimulatePaymentAsync(id, userId, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("付款成功 | OrderId={OrderId} | UserId={UserId}", id, userId);
            TempData[TempDataKeys.Success] = "付款成功！課程已解鎖";
            return RedirectToAction(nameof(Detail), new { id });
        }
        logger.LogWarning("付款失敗 | OrderId={OrderId} | Error={Error} | UserId={UserId}",
            id, result.ErrorMessage, userId);
        TempData[TempDataKeys.Error] = result.ErrorMessage ?? "付款失敗";
        return RedirectToAction(nameof(Checkout), new { id });
    }

    /// <summary>取消訂單（POST），僅限 Pending 狀態的訂單可取消</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("使用者取消訂單 | OrderId={OrderId} | UserId={UserId}", id, userId);
        var result = await orderService.CancelOrderAsync(id, userId, ct);
        if (result.IsSuccess)
            logger.LogInformation("取消訂單成功 | OrderId={OrderId} | UserId={UserId}", id, userId);
        else
            logger.LogWarning("取消訂單失敗 | OrderId={OrderId} | Error={Error} | UserId={UserId}",
                id, result.ErrorMessage, userId);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "訂單已取消" : (result.ErrorMessage ?? "取消失敗");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>訂單詳情頁，顯示單筆訂單的完整資訊</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("查看訂單詳情 | OrderId={OrderId} | UserId={UserId}", id, userId);
        var order = await orderService.GetOrderDetailAsync(id, userId, ct);
        if (order is null)
        {
            logger.LogWarning("訂單不存在 | OrderId={OrderId} | UserId={UserId}", id, userId);
            return NotFound();
        }
        return View(order);
    }
}
