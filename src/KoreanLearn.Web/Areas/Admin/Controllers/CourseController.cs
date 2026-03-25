using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CourseController(ICourseAdminService courseAdminService) : Controller
{
    private const int PageSize = 20;

    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await courseAdminService.GetCoursesPagedAsync(page, PageSize, ct);
        return View(result);
    }

    public IActionResult Create()
    {
        return View(new CreateCourseViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await courseAdminService.CreateCourseAsync(vm, ct);
        if (result is { IsSuccess: true, Data: var courseId })
        {
            if (vm.CoverImage is not null)
            {
                var coverPath = await SaveCoverImageAsync(vm.CoverImage);
                await courseAdminService.UpdateCourseImageAsync(courseId, coverPath, ct);
            }
            TempData["Success"] = "課程建立成功";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetCourseDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetCourseForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCourseViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.CoverImage is not null)
        {
            var coverPath = await SaveCoverImageAsync(vm.CoverImage);
            await courseAdminService.UpdateCourseImageAsync(vm.Id, coverPath, ct);
        }

        var result = await courseAdminService.UpdateCourseAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "課程更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await courseAdminService.DeleteCourseAsync(id, ct);
        if (result.IsSuccess)
            TempData["Success"] = "課程已刪除";
        else
            TempData["Error"] = result.ErrorMessage ?? "刪除失敗";

        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveCoverImageAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/covers/{fileName}";
    }
}
