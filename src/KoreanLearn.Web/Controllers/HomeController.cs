using System.Diagnostics;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

public class HomeController(ICourseService courseService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
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
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
