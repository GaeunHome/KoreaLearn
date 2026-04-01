using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Section;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台章節管理 Controller，提供課程章節的新增、編輯與刪除</summary>
public class SectionController(ICourseAdminService courseAdminService, ILogger<SectionController> logger) : AdminBaseController
{
    /// <summary>新增章節表單頁（GET），預帶所屬課程資訊</summary>
    public IActionResult Create(int courseId, string? courseTitle)
    {
        logger.LogInformation("管理員進入新增章節頁面 | CourseId={CourseId} | UserId={UserId}", courseId, GetCurrentUserId());
        var vm = new SectionFormViewModel
        {
            CourseId = courseId,
            CourseTitle = courseTitle
        };
        return View(vm);
    }

    /// <summary>新增章節（POST），成功後導回課程詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SectionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增章節失敗：模型驗證錯誤 | CourseId={CourseId} | UserId={UserId}", vm.CourseId, GetCurrentUserId());
            return View(vm);
        }

        var result = await courseAdminService.CreateSectionAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員新增章節成功 | CourseId={CourseId} | UserId={UserId}", vm.CourseId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "章節建立成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

        logger.LogWarning("管理員新增章節失敗 | CourseId={CourseId} | Error={Error} | UserId={UserId}", vm.CourseId, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯章節表單頁（GET），載入現有章節資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯章節頁面 | SectionId={SectionId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await courseAdminService.GetSectionForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看章節失敗：資料不存在 | SectionId={SectionId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新章節（POST），成功後導回課程詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SectionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新章節失敗：模型驗證錯誤 | SectionId={SectionId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            return View(vm);
        }

        var result = await courseAdminService.UpdateSectionAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新章節成功 | SectionId={SectionId} | CourseId={CourseId} | UserId={UserId}", vm.Id, vm.CourseId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "章節更新成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

        logger.LogWarning("管理員更新章節失敗 | SectionId={SectionId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除章節（POST，軟刪除），完成後導回課程詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var result = await courseAdminService.DeleteSectionAsync(id, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員刪除章節 | SectionId={SectionId} | CourseId={CourseId} | UserId={UserId}", id, courseId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "章節已刪除";
        }
        else
        {
            logger.LogWarning("管理員刪除章節失敗 | SectionId={SectionId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
            TempData[TempDataKeys.Error] = result.ErrorMessage ?? "刪除失敗";
        }

        return RedirectToAction("Detail", "Course", new { area = "Admin", id = courseId });
    }
}
