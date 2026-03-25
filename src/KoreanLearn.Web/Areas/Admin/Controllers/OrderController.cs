using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台訂單管理 Controller，提供全站訂單的分頁瀏覽</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrderController(IOrderService orderService) : Controller
{
    /// <summary>訂單列表頁（分頁），顯示全站所有訂單</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await orderService.GetAllOrdersPagedAsync(page, 20, ct);
        return View(result);
    }
}
