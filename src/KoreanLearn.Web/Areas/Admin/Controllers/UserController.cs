using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserController(IUserManagementService userService) : Controller
{
    private const int PageSize = 20;

    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken ct = default)
    {
        var result = await userService.GetUsersPagedAsync(search, page, PageSize, ct);
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> Detail(string id, CancellationToken ct = default)
    {
        var vm = await userService.GetUserDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToTeacher(string id, CancellationToken ct = default)
    {
        var result = await userService.PromoteToTeacherAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "已升級為教師" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoteFromTeacher(string id, CancellationToken ct = default)
    {
        var result = await userService.DemoteFromTeacherAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "已從教師降級" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Detail), new { id });
    }
}
