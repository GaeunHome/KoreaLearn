using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

[Area("Learn")]
[Authorize]
public class LessonController(
    ILessonPlayerService lessonPlayerService,
    IProgressService progressService) : Controller
{
    public async Task<IActionResult> Video(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await lessonPlayerService.GetVideoPlayerAsync(id, userId, ct);
        if (vm is null) return AccessDeniedOrNotFound(id);
        return View(vm);
    }

    public async Task<IActionResult> Article(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await lessonPlayerService.GetArticlePlayerAsync(id, userId, ct);
        if (vm is null) return AccessDeniedOrNotFound(id);
        return View(vm);
    }

    public async Task<IActionResult> Pdf(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await lessonPlayerService.GetPdfPlayerAsync(id, userId, ct);
        if (vm is null) return AccessDeniedOrNotFound(id);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> SaveProgress(
        [FromBody] SaveProgressRequest request, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await progressService.SaveVideoProgressAsync(
            userId, request.LessonId, request.ProgressSeconds, ct);

        if (result.IsSuccess)
            return Json(new { success = true, progressSeconds = result.Data });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await progressService.MarkLessonCompleteAsync(userId, id, ct);

        if (result.IsSuccess)
            return Json(new { success = true });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    [HttpPost]
    public async Task<IActionResult> UndoComplete(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await progressService.UndoLessonCompleteAsync(userId, id, ct);

        if (result.IsSuccess)
            return Json(new { success = true });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    private IActionResult AccessDeniedOrNotFound(int lessonId)
    {
        TempData["Warning"] = "您尚未購買此課程，無法存取此單元內容。請先購買課程。";
        return RedirectToAction("Index", "Course", new { area = "" });
    }
}

public class SaveProgressRequest
{
    public int LessonId { get; set; }
    public int ProgressSeconds { get; set; }
}
