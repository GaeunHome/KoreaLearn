using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Lesson;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class LessonController(ICourseAdminService courseAdminService) : Controller
{
    public IActionResult Create(int sectionId, int courseId, string? sectionTitle, string? courseTitle)
    {
        var vm = new LessonFormViewModel
        {
            SectionId = sectionId,
            CourseId = courseId,
            SectionTitle = sectionTitle,
            CourseTitle = courseTitle
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        await HandleFileUploadsAsync(vm);

        var result = await courseAdminService.CreateLessonAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "單元建立成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetLessonForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        await HandleFileUploadsAsync(vm);

        var result = await courseAdminService.UpdateLessonAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "單元更新成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var result = await courseAdminService.DeleteLessonAsync(id, ct);
        if (result.IsSuccess)
            TempData["Success"] = "單元已刪除";
        else
            TempData["Error"] = result.ErrorMessage ?? "刪除失敗";

        return RedirectToAction("Detail", "Course", new { area = "Admin", id = courseId });
    }

    private async Task HandleFileUploadsAsync(LessonFormViewModel vm)
    {
        if (vm.VideoFile is not null)
        {
            var url = await SaveFileAsync(vm.VideoFile, "videos");
            vm.ExistingVideoUrl = url;
        }

        if (vm.PdfFile is not null)
        {
            var url = await SaveFileAsync(vm.PdfFile, "pdfs");
            vm.ExistingPdfUrl = url;
            vm.ExistingPdfFileName = vm.PdfFile.FileName;
        }
    }

    private static async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }
}
