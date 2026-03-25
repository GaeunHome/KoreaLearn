using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController(IDashboardService dashboardService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var vm = await dashboardService.GetAdminDashboardAsync(ct);
        return View(vm);
    }
}
