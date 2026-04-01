using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Web.Infrastructure;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師課程管理 Controller，提供教師自有課程的 CRUD 操作（含封面圖上傳）</summary>
public class CourseController(ITeacherCourseService teacherService, IFileUploadService fileUploadService, ILogger<CourseController> logger) : TeacherBaseController
{
    /// <summary>教師課程列表頁（分頁），僅顯示該教師建立的課程</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("教師查看課程列表 | Page={Page} | TeacherId={TeacherId}", page, TeacherId);
        var result = await teacherService.GetTeacherCoursesPagedAsync(TeacherId, page, DisplayConstants.TeacherPageSize, ct);
        return View(result);
    }

    /// <summary>新增課程表單頁（GET）</summary>
    public IActionResult Create()
    {
        logger.LogInformation("教師進入新增課程頁面 | TeacherId={TeacherId}", TeacherId);
        return View(new CourseFormViewModel());
    }

    /// <summary>新增課程（POST），建立課程並儲存封面圖片，成功後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("教師新增課程失敗：模型驗證錯誤 | TeacherId={TeacherId}", TeacherId);
            return View(vm);
        }

        var result = await teacherService.CreateCourseAsync(vm, TeacherId, ct);
        if (result is { IsSuccess: true, Data: var courseId })
        {
            if (vm.CoverImage is not null)
            {
                var coverPath = await fileUploadService.SaveAsync(vm.CoverImage, "covers");
                await teacherService.UpdateCourseImageAsync(courseId, coverPath, TeacherId, ct);
            }
            logger.LogInformation("教師新增課程成功 | CourseId={CourseId} | TeacherId={TeacherId}", courseId, TeacherId);
            TempData[TempDataKeys.Success] = "課程建立成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("教師新增課程失敗 | Error={Error} | TeacherId={TeacherId}", result.ErrorMessage, TeacherId);
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>課程詳情頁，顯示教師自有課程的完整資訊與章節結構</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("教師查看課程詳情 | CourseId={CourseId} | TeacherId={TeacherId}", id, TeacherId);
        var vm = await teacherService.GetCourseDetailAsync(id, TeacherId, ct);
        if (vm is null)
        {
            logger.LogWarning("教師操作課程失敗：無權限或不存在 | CourseId={CourseId} | TeacherId={TeacherId}", id, TeacherId);
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>編輯課程表單頁（GET），載入教師自有課程資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("教師進入編輯課程頁面 | CourseId={CourseId} | TeacherId={TeacherId}", id, TeacherId);
        var vm = await teacherService.GetCourseForEditAsync(id, TeacherId, ct);
        if (vm is null)
        {
            logger.LogWarning("教師操作課程失敗：無權限或不存在 | CourseId={CourseId} | TeacherId={TeacherId}", id, TeacherId);
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
            logger.LogWarning("教師更新課程失敗：模型驗證錯誤 | CourseId={CourseId} | TeacherId={TeacherId}", vm.Id, TeacherId);
            return View(vm);
        }

        if (vm.CoverImage is not null)
        {
            var coverPath = await fileUploadService.SaveAsync(vm.CoverImage, "covers");
            await teacherService.UpdateCourseImageAsync(vm.Id, coverPath, TeacherId, ct);
        }

        var result = await teacherService.UpdateCourseAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("教師更新課程成功 | CourseId={CourseId} | TeacherId={TeacherId}", vm.Id, TeacherId);
            TempData[TempDataKeys.Success] = "課程更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }

        logger.LogWarning("教師更新課程失敗 | Error={Error} | CourseId={CourseId} | TeacherId={TeacherId}", result.ErrorMessage, vm.Id, TeacherId);
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除課程（POST，軟刪除），僅能刪除教師自有課程，完成後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await teacherService.DeleteCourseAsync(id, TeacherId, ct);
        if (result.IsSuccess)
            logger.LogInformation("教師刪除課程成功 | CourseId={CourseId} | TeacherId={TeacherId}", id, TeacherId);
        else
            logger.LogWarning("教師刪除課程失敗 | Error={Error} | CourseId={CourseId} | TeacherId={TeacherId}", result.ErrorMessage, id, TeacherId);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "課程已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

}
