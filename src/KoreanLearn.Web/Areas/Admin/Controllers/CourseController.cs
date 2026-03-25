using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台課程管理 Controller，提供課程的 CRUD 操作（含封面圖上傳）</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CourseController(ICourseAdminService courseAdminService) : Controller
{
    private const int PageSize = 20;

    /// <summary>課程列表頁（分頁），顯示所有課程的管理清單</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await courseAdminService.GetCoursesPagedAsync(page, PageSize, ct);
        return View(result);
    }

    /// <summary>新增課程表單頁（GET）</summary>
    public IActionResult Create()
    {
        return View(new CreateCourseViewModel());
    }

    /// <summary>新增課程（POST），建立課程並儲存封面圖片，成功後導回列表</summary>
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

    /// <summary>課程詳情頁，顯示課程完整資訊與章節結構</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetCourseDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>編輯課程表單頁（GET），載入現有課程資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetCourseForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新課程（POST），更新課程資料與封面圖片，成功後導回詳情頁</summary>
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

    /// <summary>刪除課程（POST，軟刪除），完成後導回列表</summary>
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

    /// <summary>儲存封面圖片至 wwwroot/uploads/covers/，回傳相對路徑</summary>
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
