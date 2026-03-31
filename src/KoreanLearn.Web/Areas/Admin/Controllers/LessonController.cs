using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台單元管理 Controller，提供課程單元的新增、編輯與刪除（含影片/PDF 檔案上傳）</summary>
public class LessonController(ICourseAdminService courseAdminService, IFileUploadService fileUploadService) : AdminBaseController
{
    /// <summary>新增單元表單頁（GET），預帶所屬章節與課程資訊</summary>
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

    /// <summary>新增單元（POST），處理檔案上傳後建立單元，成功導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        await HandleFileUploadsAsync(vm);

        var result = await courseAdminService.CreateLessonAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "單元建立成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯單元表單頁（GET），載入現有單元資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await courseAdminService.GetLessonForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新單元（POST），處理檔案上傳後更新單元，成功導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        await HandleFileUploadsAsync(vm);

        var result = await courseAdminService.UpdateLessonAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "單元更新成功";
            return RedirectToAction("Detail", "Course", new { area = "Admin", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除單元（POST，軟刪除），完成後導回課程詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var result = await courseAdminService.DeleteLessonAsync(id, ct);
        if (result.IsSuccess)
            TempData[TempDataKeys.Success] = "單元已刪除";
        else
            TempData[TempDataKeys.Error] = result.ErrorMessage ?? "刪除失敗";

        return RedirectToAction("Detail", "Course", new { area = "Admin", id = courseId });
    }

    /// <summary>處理影片與 PDF 檔案上傳，將檔案存入對應資料夾並更新 ViewModel 的 URL</summary>
    private async Task HandleFileUploadsAsync(LessonFormViewModel vm)
    {
        if (vm.VideoFile is not null)
            vm.ExistingVideoUrl = await fileUploadService.SaveAsync(vm.VideoFile, "videos");

        if (vm.PdfFile is not null)
        {
            vm.ExistingPdfUrl = await fileUploadService.SaveAsync(vm.PdfFile, "pdfs");
            vm.ExistingPdfFileName = vm.PdfFile.FileName;
        }
    }
}
