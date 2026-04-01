using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Web.Infrastructure;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台課程管理 Controller，提供課程的 CRUD 操作（含封面圖上傳）</summary>
public class CourseController(ICourseAdminService courseAdminService, IFileUploadService fileUploadService, ILogger<CourseController> logger) : AdminBaseController
{
    /// <summary>課程列表頁（分頁），顯示所有課程的管理清單</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看課程列表 | Page={Page} | UserId={UserId}", page, GetCurrentUserId());
        var result = await courseAdminService.GetCoursesPagedAsync(page, DisplayConstants.AdminPageSize, ct);
        return View(result);
    }

    /// <summary>新增課程表單頁（GET）</summary>
    public IActionResult Create()
    {
        logger.LogInformation("管理員進入新增課程頁面 | UserId={UserId}", GetCurrentUserId());
        return View(new CourseFormViewModel());
    }

    /// <summary>新增課程（POST），建立課程並儲存封面圖片，成功後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增課程失敗：模型驗證錯誤 | UserId={UserId}", GetCurrentUserId());
            return View(vm);
        }

        var result = await courseAdminService.CreateCourseAsync(vm, ct);
        if (result is { IsSuccess: true, Data: var courseId })
        {
            if (vm.CoverImage is not null)
            {
                var coverPath = await fileUploadService.SaveAsync(vm.CoverImage, "covers");
                await courseAdminService.UpdateCourseImageAsync(courseId, coverPath, ct);
            }
            logger.LogInformation("管理員新增課程成功 | CourseId={CourseId} | UserId={UserId}", courseId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "課程建立成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("管理員新增課程失敗 | Error={Error} | UserId={UserId}", result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>課程詳情頁，顯示課程完整資訊與章節結構</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看課程詳情 | CourseId={CourseId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await courseAdminService.GetCourseDetailAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看課程失敗：資料不存在 | CourseId={CourseId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>編輯課程表單頁（GET），載入現有課程資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯課程頁面 | CourseId={CourseId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await courseAdminService.GetCourseForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看課程失敗：資料不存在 | CourseId={CourseId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新課程（POST），更新課程資料與封面圖片，成功後導回詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新課程失敗：模型驗證錯誤 | CourseId={CourseId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            return View(vm);
        }

        if (vm.CoverImage is not null)
        {
            var coverPath = await fileUploadService.SaveAsync(vm.CoverImage, "covers");
            await courseAdminService.UpdateCourseImageAsync(vm.Id, coverPath, ct);
        }

        var result = await courseAdminService.UpdateCourseAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新課程成功 | CourseId={CourseId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "課程更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }

        logger.LogWarning("管理員更新課程失敗 | CourseId={CourseId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除課程（POST，軟刪除），完成後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await courseAdminService.DeleteCourseAsync(id, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員刪除課程 | CourseId={CourseId} | UserId={UserId}", id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "課程已刪除";
        }
        else
        {
            logger.LogWarning("管理員刪除課程失敗 | CourseId={CourseId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
            TempData[TempDataKeys.Error] = result.ErrorMessage ?? "刪除失敗";
        }

        return RedirectToAction(nameof(Index));
    }

}
