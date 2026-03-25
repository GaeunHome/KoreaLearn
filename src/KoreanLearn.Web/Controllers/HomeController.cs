using System.Diagnostics;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

public class HomeController(
    ICourseService courseService,
    ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        logger.LogInformation("載入首頁 | User={User}", User.Identity?.Name ?? "Anonymous");
        var vm = await courseService.GetHomeViewModelAsync(ct);
        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        logger.LogError("錯誤頁面被觸發 | RequestId={RequestId}", requestId);
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
