using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Section;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師章節管理 Controller，提供教師自有課程章節的新增、編輯與刪除</summary>
public class SectionController(ITeacherCourseService teacherService, ILogger<SectionController> logger) : TeacherBaseController
{
    /// <summary>新增章節表單頁（GET），預帶所屬課程資訊</summary>
    public IActionResult Create(int courseId, string? courseTitle)
    {
        logger.LogInformation("教師進入新增章節頁面 | CourseId={CourseId} | TeacherId={TeacherId}", courseId, TeacherId);
        return View(new SectionFormViewModel { CourseId = courseId, CourseTitle = courseTitle });
    }

    /// <summary>新增章節（POST），驗證教師身份後建立章節，成功導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SectionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("教師新增章節失敗：模型驗證錯誤 | CourseId={CourseId} | TeacherId={TeacherId}", vm.CourseId, TeacherId);
            return View(vm);
        }

        var result = await teacherService.CreateSectionAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("教師新增章節成功 | CourseId={CourseId} | TeacherId={TeacherId}", vm.CourseId, TeacherId);
            TempData[TempDataKeys.Success] = "章節建立成功";
            return RedirectToAction("Detail", "Course", new { area = "Teacher", id = vm.CourseId });
        }

        logger.LogWarning("教師新增章節失敗 | Error={Error} | CourseId={CourseId} | TeacherId={TeacherId}", result.ErrorMessage, vm.CourseId, TeacherId);
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯章節表單頁（GET），載入教師自有課程的章節資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("教師進入編輯章節頁面 | SectionId={SectionId} | TeacherId={TeacherId}", id, TeacherId);
        var vm = await teacherService.GetSectionForEditAsync(id, TeacherId, ct);
        if (vm is null)
        {
            logger.LogWarning("教師操作章節失敗：無權限或不存在 | SectionId={SectionId} | TeacherId={TeacherId}", id, TeacherId);
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新章節（POST），驗證教師身份後更新章節，成功導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SectionFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("教師更新章節失敗：模型驗證錯誤 | SectionId={SectionId} | TeacherId={TeacherId}", vm.Id, TeacherId);
            return View(vm);
        }

        var result = await teacherService.UpdateSectionAsync(vm, TeacherId, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("教師更新章節成功 | SectionId={SectionId} | CourseId={CourseId} | TeacherId={TeacherId}", vm.Id, vm.CourseId, TeacherId);
            TempData[TempDataKeys.Success] = "章節更新成功";
            return RedirectToAction("Detail", "Course", new { area = "Teacher", id = vm.CourseId });
        }

        logger.LogWarning("教師更新章節失敗 | Error={Error} | SectionId={SectionId} | TeacherId={TeacherId}", result.ErrorMessage, vm.Id, TeacherId);
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除章節（POST，軟刪除），驗證教師身份後刪除，完成導回課程詳情</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var result = await teacherService.DeleteSectionAsync(id, TeacherId, ct);
        if (result.IsSuccess)
            logger.LogInformation("教師刪除章節成功 | SectionId={SectionId} | CourseId={CourseId} | TeacherId={TeacherId}", id, courseId, TeacherId);
        else
            logger.LogWarning("教師刪除章節失敗 | Error={Error} | SectionId={SectionId} | TeacherId={TeacherId}", result.ErrorMessage, id, TeacherId);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "章節已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction("Detail", "Course", new { area = "Teacher", id = courseId });
    }
}
