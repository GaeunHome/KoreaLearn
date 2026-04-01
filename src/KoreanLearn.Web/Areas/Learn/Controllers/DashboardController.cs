using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>學生儀表板 Controller，顯示個人學習摘要（我的課程、進度、連續學習天數）</summary>
public class DashboardController(
    IDashboardService dashboardService,
    ILogger<DashboardController> logger) : LearnBaseController
{
    /// <summary>學生儀表板首頁，顯示該使用者的學習進度與統計</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("學生查看儀表板 | UserId={UserId}", userId);
        var vm = await dashboardService.GetStudentDashboardAsync(userId, ct);
        return View(vm);
    }
}
