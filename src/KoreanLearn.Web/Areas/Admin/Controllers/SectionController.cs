using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Section;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SectionController(ICourseAdminService courseAdminService) : Controller
{
    public IActionResult Create(int courseId, string? courseTitle)
    {
        var vm = new SectionFormViewModel
        {
            CourseId = courseId,
            CourseTitle = courseTitle
        };
        return View(vm);
    }

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

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetSectionForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

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
