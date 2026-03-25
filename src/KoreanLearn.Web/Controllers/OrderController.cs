using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Controllers;

/// <summary>訂單 Controller，處理使用者的訂單列表、建立訂單、結帳與模擬付款流程</summary>
[Authorize]
public class OrderController(IOrderService orderService) : Controller
{
    /// <summary>我的訂單列表（分頁），顯示當前使用者的所有訂單</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await orderService.GetUserOrdersAsync(userId, page, 10, ct);
        return View(result);
    }

    /// <summary>建立訂單（POST），成功後導向結帳頁；失敗導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await orderService.CreateOrderAsync(userId, courseId, ct);
        if (result is { IsSuccess: true, Data: var orderId })
            return RedirectToAction(nameof(Checkout), new { id = orderId });

        TempData["Error"] = result.ErrorMessage ?? "建立訂單失敗";
        return RedirectToAction("Detail", "Course", new { id = courseId });
    }

    /// <summary>結帳頁面，顯示訂單明細與付款按鈕；找不到訂單時回傳 404</summary>
    public async Task<IActionResult> Checkout(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await orderService.GetOrderDetailAsync(id, userId, ct);
        if (order is null) return NotFound();
        return View(order);
    }

    /// <summary>模擬付款（POST），成功後解鎖課程並導向訂單詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await orderService.SimulatePaymentAsync(id, userId, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "付款成功！課程已解鎖";
            return RedirectToAction(nameof(Detail), new { id });
        }
        TempData["Error"] = result.ErrorMessage ?? "付款失敗";
        return RedirectToAction(nameof(Checkout), new { id });
    }

    /// <summary>訂單詳情頁，顯示單筆訂單的完整資訊</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await orderService.GetOrderDetailAsync(id, userId, ct);
        if (order is null) return NotFound();
        return View(order);
    }
}
