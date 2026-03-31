using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>學習單元播放 Controller，提供影片、文章、PDF 的沉浸式學習介面與進度追蹤</summary>
public class LessonController(
    ILessonPlayerService lessonPlayerService,
    IProgressService progressService) : LearnBaseController
{
    /// <summary>影片單元播放頁，載入 HTML5 video 播放器與已儲存的觀看進度</summary>
    public async Task<IActionResult> Video(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var vm = await lessonPlayerService.GetVideoPlayerAsync(id, userId, GetUserRoles(), ct);
        if (vm is null) return AccessDeniedOrNotFound();
        return View(vm);
    }

    /// <summary>文章單元閱讀頁，顯示富文字內容</summary>
    public async Task<IActionResult> Article(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var vm = await lessonPlayerService.GetArticlePlayerAsync(id, userId, GetUserRoles(), ct);
        if (vm is null) return AccessDeniedOrNotFound();
        return View(vm);
    }

    /// <summary>PDF 單元頁面，提供 PDF 檢視與下載（需已購買課程）</summary>
    public async Task<IActionResult> Pdf(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var vm = await lessonPlayerService.GetPdfPlayerAsync(id, userId, GetUserRoles(), ct);
        if (vm is null) return AccessDeniedOrNotFound();
        return View(vm);
    }

    /// <summary>儲存影片觀看進度（POST，JSON），回傳目前進度秒數</summary>
    [HttpPost]
    public async Task<IActionResult> SaveProgress(
        [FromBody] SaveProgressRequest request, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await progressService.SaveVideoProgressAsync(
            userId, request.LessonId, request.ProgressSeconds, GetUserRoles(), ct);

        if (result.IsSuccess)
            return Json(new { success = true, progressSeconds = result.Data });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>標記單元為已完成（POST），回傳 JSON 結果</summary>
    [HttpPost]
    public async Task<IActionResult> Complete(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await progressService.MarkLessonCompleteAsync(userId, id, GetUserRoles(), ct);

        if (result.IsSuccess)
            return Json(new { success = true });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>取消單元完成狀態（POST），回傳 JSON 結果</summary>
    [HttpPost]
    public async Task<IActionResult> UndoComplete(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await progressService.UndoLessonCompleteAsync(userId, id, GetUserRoles(), ct);

        if (result.IsSuccess)
            return Json(new { success = true });

        return Json(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>取得當前使用者的所有角色</summary>
    private IEnumerable<string> GetUserRoles()
        => User.FindAll(ClaimTypes.Role).Select(c => c.Value);

    /// <summary>未購買或找不到單元時，導向課程列表並顯示提示訊息</summary>
    private IActionResult AccessDeniedOrNotFound()
    {
        TempData[TempDataKeys.Warning] = "您尚未購買此課程，無法存取此單元內容。請先購買課程。";
        return RedirectToAction("Index", "Course", new { area = "" });
    }
}
