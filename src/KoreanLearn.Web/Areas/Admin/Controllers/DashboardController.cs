using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台儀表板 Controller，顯示系統摘要統計（總用戶數、總收入、熱門課程等）</summary>
public class DashboardController(IDashboardService dashboardService) : AdminBaseController
{
    /// <summary>儀表板首頁，透過平行查詢取得各項統計數據</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var vm = await dashboardService.GetAdminDashboardAsync(ct);
        return View(vm);
    }
}
