using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>學習單元播放 Controller，提供影片、文章、PDF 的沉浸式學習介面與進度追蹤</summary>
[Area("Learn")]
[Authorize]
public class LessonController(
    ILessonPlayerService lessonPlayerService,
    IProgressService progressService) : Controller
{
    /// <summary>影片單元播放頁，載入 HTML5 video 播放器與已儲存的觀看進度</summary>
    public async Task<IActionResult> Video(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await lessonPlayerService.GetVideoPlayerAsync(id, userId, ct);
        if (vm is null) return AccessDeniedOrNotFound(id);
        return View(vm);
    }

    /// <summary>文章單元閱讀頁，顯示富文字內容</summary>
    public async Task<IActionResult> Article(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await lessonPlayerService.GetArticlePlayerAsync(id, userId, ct);
        if (vm is null) return AccessDeniedOrNotFound(id);
        return View(vm);
    }

    /// <summary>PDF 單元頁面，提供 PDF 檢視與下載（需已購買課程）</summary>
    public async Task<IActionResult> Pdf(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await lessonPlayerService.GetPdfPlayerAsync(id, userId, ct);
        if (vm is null) return AccessDeniedOrNotFound(id);
        return View(vm);
    }

    /// <summary>儲存影片觀看進度（POST，JSON），回傳目前進度秒數</summary>
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

    /// <summary>標記單元為已完成（POST），回傳 JSON 結果</summary>
    [HttpPost]
    public async Task<IActionResult> Complete(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await progressService.MarkLessonCompleteAsync(userId, id, ct);

        if (result.IsSuccess)
            return Json(new { success = true });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>取消單元完成狀態（POST），回傳 JSON 結果</summary>
    [HttpPost]
    public async Task<IActionResult> UndoComplete(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await progressService.UndoLessonCompleteAsync(userId, id, ct);

        if (result.IsSuccess)
            return Json(new { success = true });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>未購買或找不到單元時，導向課程列表並顯示提示訊息</summary>
    private IActionResult AccessDeniedOrNotFound(int lessonId)
    {
        TempData["Warning"] = "您尚未購買此課程，無法存取此單元內容。請先購買課程。";
        return RedirectToAction("Index", "Course", new { area = "" });
    }
}

/// <summary>影片進度儲存請求模型</summary>
public class SaveProgressRequest
{
    /// <summary>單元 ID</summary>
    public int LessonId { get; set; }
    /// <summary>觀看進度（秒）</summary>
    public int ProgressSeconds { get; set; }
}
