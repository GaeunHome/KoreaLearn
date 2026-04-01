using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台訂單管理 Controller，提供全站訂單的分頁瀏覽、詳情與狀態管理</summary>
public class OrderController(IOrderService orderService, ILogger<OrderController> logger) : AdminBaseController
{
    /// <summary>訂單列表頁（分頁），顯示全站所有訂單</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看訂單列表 | Page={Page} | UserId={UserId}", page, GetCurrentUserId());
        var result = await orderService.GetAllOrdersPagedAsync(page, DisplayConstants.AdminPageSize, ct);
        return View(result);
    }

    /// <summary>訂單詳情頁，顯示訂單完整資訊</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看訂單詳情 | OrderId={OrderId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await orderService.GetOrderDetailForAdminAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看訂單失敗：資料不存在 | OrderId={OrderId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新訂單狀態（POST），管理員操作</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, CancellationToken ct = default)
    {
        var result = await orderService.UpdateOrderStatusAsync(id, status, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員更新訂單狀態成功 | OrderId={OrderId} | Status={Status} | UserId={UserId}", id, status, GetCurrentUserId());
        else
            logger.LogWarning("管理員更新訂單狀態失敗 | OrderId={OrderId} | Status={Status} | Error={Error} | UserId={UserId}", id, status, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "訂單狀態已更新" : (result.ErrorMessage ?? "更新失敗");
        return RedirectToAction(nameof(Detail), new { id });
    }
}
