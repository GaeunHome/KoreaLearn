using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台儀表板 Controller，顯示系統摘要統計（總用戶數、總收入、熱門課程等）</summary>
public class DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : AdminBaseController
{
    /// <summary>儀表板首頁，透過平行查詢取得各項統計數據</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看儀表板 | UserId={UserId}", GetCurrentUserId());
        var vm = await dashboardService.GetAdminDashboardAsync(ct);
        return View(vm);
    }
}
