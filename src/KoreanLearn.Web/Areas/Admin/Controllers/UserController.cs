using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台使用者管理 Controller，提供使用者列表、詳情與角色升降級</summary>
public class UserController(IUserManagementService userService) : AdminBaseController
{
    /// <summary>使用者列表頁（支援搜尋與分頁）</summary>
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken ct = default)
    {
        var result = await userService.GetUsersPagedAsync(search, page, DisplayConstants.UserPageSize, ct);
        ViewBag.Search = search;
        return View(result);
    }

    /// <summary>使用者詳情頁，顯示帳號資訊與角色狀態</summary>
    public async Task<IActionResult> Detail(string id, CancellationToken ct = default)
    {
        var vm = await userService.GetUserDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>將使用者升級為教師角色（POST），完成後導回詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToTeacher(string id, CancellationToken ct = default)
    {
        var result = await userService.PromoteToTeacherAsync(id, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "已升級為教師" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Detail), new { id });
    }

    /// <summary>將使用者從教師角色降級（POST），完成後導回詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoteFromTeacher(string id, CancellationToken ct = default)
    {
        var result = await userService.DemoteFromTeacherAsync(id, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "已從教師降級" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Detail), new { id });
    }
}
