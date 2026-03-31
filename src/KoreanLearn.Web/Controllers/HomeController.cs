using System.Diagnostics;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

/// <summary>首頁 Controller，提供網站首頁、隱私權頁面與錯誤頁面</summary>
public class HomeController(
    ICourseService courseService,
    ILogger<HomeController> logger) : BaseController
{
    /// <summary>首頁：顯示精選課程與網站摘要資訊</summary>
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        logger.LogInformation("載入首頁 | User={User}", User.Identity?.Name ?? "Anonymous");
        var vm = await courseService.GetHomeViewModelAsync(ct);
        return View(vm);
    }

    /// <summary>隱私權政策頁面</summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>通用錯誤頁面，顯示 RequestId 供除錯追蹤</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        logger.LogError("錯誤頁面被觸發 | RequestId={RequestId}", requestId);
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
