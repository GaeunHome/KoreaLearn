using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class CourseController(ITeacherCourseService teacherService) : Controller
{
    private const int PageSize = 20;
    private string TeacherId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await teacherService.GetTeacherCoursesPagedAsync(TeacherId, page, PageSize, ct);
        return View(result);
    }

    public IActionResult Create() => View(new CreateCourseViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await teacherService.CreateCourseAsync(vm, TeacherId, ct);
        if (result is { IsSuccess: true, Data: var courseId })
        {
            if (vm.CoverImage is not null)
            {
                var coverPath = await SaveCoverImageAsync(vm.CoverImage);
                await teacherService.UpdateCourseImageAsync(courseId, coverPath, TeacherId, ct);
            }
            TempData["Success"] = "課程建立成功";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await teacherService.GetCourseDetailAsync(id, TeacherId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await teacherService.GetCourseForEditAsync(id, TeacherId, ct);
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
            await teacherService.UpdateCourseImageAsync(vm.Id, coverPath, TeacherId, ct);
        }

        var result = await teacherService.UpdateCourseAsync(vm, TeacherId, ct);
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
        var result = await teacherService.DeleteCourseAsync(id, TeacherId, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "課程已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    private static async Task<string> SaveCoverImageAsync(IFormFile file)
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
