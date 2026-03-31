using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師單元管理 Controller，提供教師自有課程單元的新增、編輯與刪除（含影片/PDF 上傳）</summary>
public class LessonController(ITeacherCourseService teacherService, IFileUploadService fileUploadService) : TeacherBaseController
{
    /// <summary>新增單元表單頁（GET），預帶所屬章節與課程資訊</summary>
    public IActionResult Create(int sectionId, int courseId, string? sectionTitle, string? courseTitle)
    {
        return View(new LessonFormViewModel
        {
            SectionId = sectionId, CourseId = courseId,
            SectionTitle = sectionTitle, CourseTitle = courseTitle
        });
    }

    /// <summary>新增單元（POST），處理檔案上傳後建立單元，成功導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        await HandleFileUploadsAsync(vm);

        var result = await teacherService.CreateLessonAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "單元建立成功";
            return RedirectToAction("Detail", "Course", new { area = "Teacher", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯單元表單頁（GET），載入教師自有課程的單元資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await teacherService.GetLessonForEditAsync(id, TeacherId, ct);
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

        var result = await teacherService.UpdateLessonAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "單元更新成功";
            return RedirectToAction("Detail", "Course", new { area = "Teacher", id = vm.CourseId });
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除單元（POST，軟刪除），驗證教師身份後刪除，完成導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var result = await teacherService.DeleteLessonAsync(id, TeacherId, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "單元已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction("Detail", "Course", new { area = "Teacher", id = courseId });
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
