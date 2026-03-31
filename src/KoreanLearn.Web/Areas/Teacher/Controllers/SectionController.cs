using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Section;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師章節管理 Controller，提供教師自有課程章節的新增、編輯與刪除</summary>
public class SectionController(ITeacherCourseService teacherService) : TeacherBaseController
{
    /// <summary>新增章節表單頁（GET），預帶所屬課程資訊</summary>
    public IActionResult Create(int courseId, string? courseTitle)
    {
        return View(new SectionFormViewModel { CourseId = courseId, CourseTitle = courseTitle });
    }

    /// <summary>新增章節（POST），驗證教師身份後建立章節，成功導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SectionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await teacherService.CreateSectionAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "章節建立成功";
            return RedirectToAction("Detail", "Course", new { area = "Teacher", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯章節表單頁（GET），載入教師自有課程的章節資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await teacherService.GetSectionForEditAsync(id, TeacherId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新章節（POST），驗證教師身份後更新章節，成功導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SectionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await teacherService.UpdateSectionAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "章節更新成功";
            return RedirectToAction("Detail", "Course", new { area = "Teacher", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除章節（POST，軟刪除），驗證教師身份後刪除，完成導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var result = await teacherService.DeleteSectionAsync(id, TeacherId, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "章節已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction("Detail", "Course", new { area = "Teacher", id = courseId });
    }
}
