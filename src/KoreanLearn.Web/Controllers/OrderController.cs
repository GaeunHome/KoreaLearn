using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Controllers;

[Authorize]
public class OrderController(IOrderService orderService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await orderService.GetUserOrdersAsync(userId, page, 10, ct);
        return View(result);
    }

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

    public async Task<IActionResult> Checkout(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await orderService.GetOrderDetailAsync(id, userId, ct);
        if (order is null) return NotFound();
        return View(order);
    }

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

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await orderService.GetOrderDetailAsync(id, userId, ct);
        if (order is null) return NotFound();
        return View(order);
    }
}
