using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>學生儀表板 Controller，顯示個人學習摘要（我的課程、進度、連續學習天數）</summary>
[Area("Learn")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : Controller
{
    /// <summary>學生儀表板首頁，顯示該使用者的學習進度與統計</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await dashboardService.GetStudentDashboardAsync(userId, ct);
        return View(vm);
    }
}
