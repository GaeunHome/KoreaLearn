using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Web.Infrastructure;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師課程管理 Controller，提供教師自有課程的 CRUD 操作（含封面圖上傳）</summary>
public class CourseController(ITeacherCourseService teacherService, IFileUploadService fileUploadService) : TeacherBaseController
{
    /// <summary>教師課程列表頁（分頁），僅顯示該教師建立的課程</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await teacherService.GetTeacherCoursesPagedAsync(TeacherId, page, DisplayConstants.TeacherPageSize, ct);
        return View(result);
    }

    /// <summary>新增課程表單頁（GET）</summary>
    public IActionResult Create() => View(new CourseFormViewModel());

    /// <summary>新增課程（POST），建立課程並儲存封面圖片，成功後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await teacherService.CreateCourseAsync(vm, TeacherId, ct);
        if (result is { IsSuccess: true, Data: var courseId })
        {
            if (vm.CoverImage is not null)
            {
                var coverPath = await fileUploadService.SaveAsync(vm.CoverImage, "covers");
                await teacherService.UpdateCourseImageAsync(courseId, coverPath, TeacherId, ct);
            }
            TempData[TempDataKeys.Success] = "課程建立成功";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>課程詳情頁，顯示教師自有課程的完整資訊與章節結構</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await teacherService.GetCourseDetailAsync(id, TeacherId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>編輯課程表單頁（GET），載入教師自有課程資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await teacherService.GetCourseForEditAsync(id, TeacherId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新課程（POST），更新課程資料與封面圖片，成功後導回詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.CoverImage is not null)
        {
            var coverPath = await fileUploadService.SaveAsync(vm.CoverImage, "covers");
            await teacherService.UpdateCourseImageAsync(vm.Id, coverPath, TeacherId, ct);
        }

        var result = await teacherService.UpdateCourseAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "課程更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除課程（POST，軟刪除），僅能刪除教師自有課程，完成後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await teacherService.DeleteCourseAsync(id, TeacherId, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "課程已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

}
