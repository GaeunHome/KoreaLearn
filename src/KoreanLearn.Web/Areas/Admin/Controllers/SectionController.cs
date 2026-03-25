using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Section;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台章節管理 Controller，提供課程章節的新增、編輯與刪除</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SectionController(ICourseAdminService courseAdminService) : Controller
{
    /// <summary>新增章節表單頁（GET），預帶所屬課程資訊</summary>
    public IActionResult Create(int courseId, string? courseTitle)
    {
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
        if (!ModelState.IsValid) return View(vm);

        var result = await courseAdminService.CreateSectionAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "章節建立成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯章節表單頁（GET），載入現有章節資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetSectionForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新章節（POST），成功後導回課程詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SectionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await courseAdminService.UpdateSectionAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "章節更新成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

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
            TempData["Success"] = "章節已刪除";
        else
            TempData["Error"] = result.ErrorMessage ?? "刪除失敗";

        return RedirectToAction("Detail", "Course", new { area = "Admin", id = courseId });
    }
}
