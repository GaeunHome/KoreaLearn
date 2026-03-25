using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

[Area("Learn")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await dashboardService.GetStudentDashboardAsync(userId, ct);
        return View(vm);
    }
}
