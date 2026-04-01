using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台使用者管理 Controller，提供使用者列表、詳情與角色升降級</summary>
public class UserController(IUserManagementService userService, ILogger<UserController> logger) : AdminBaseController
{
    /// <summary>使用者列表頁（支援搜尋與分頁）</summary>
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看使用者列表 | Search={Search} | Page={Page} | UserId={UserId}", search, page, GetCurrentUserId());
        var result = await userService.GetUsersPagedAsync(search, page, DisplayConstants.UserPageSize, ct);
        ViewBag.Search = search;
        return View(result);
    }

    /// <summary>使用者詳情頁，顯示帳號資訊與角色狀態</summary>
    public async Task<IActionResult> Detail(string id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看使用者詳情 | TargetUserId={TargetUserId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await userService.GetUserDetailAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看使用者失敗：資料不存在 | TargetUserId={TargetUserId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>將使用者升級為教師角色（POST），完成後導回詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToTeacher(string id, CancellationToken ct = default)
    {
        var result = await userService.PromoteToTeacherAsync(id, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員升級使用者為教師 | TargetUserId={TargetUserId} | UserId={UserId}", id, GetCurrentUserId());
        else
            logger.LogWarning("管理員升級使用者為教師失敗 | TargetUserId={TargetUserId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "已升級為教師" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Detail), new { id });
    }

    /// <summary>將使用者從教師角色降級（POST），完成後導回詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoteFromTeacher(string id, CancellationToken ct = default)
    {
        var result = await userService.DemoteFromTeacherAsync(id, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員將教師降級 | TargetUserId={TargetUserId} | UserId={UserId}", id, GetCurrentUserId());
        else
            logger.LogWarning("管理員將教師降級失敗 | TargetUserId={TargetUserId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "已從教師降級" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Detail), new { id });
    }
}
