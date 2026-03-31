using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Controllers;

/// <summary>討論區 Controller，提供討論列表、詳情、發文、回覆與刪除功能</summary>
public class DiscussionController(
    IDiscussionService discussionService,
    ICourseService courseService) : BaseController
{
    /// <summary>討論列表頁（支援依課程篩選與分頁），無指定課程時顯示全站討論</summary>
    public async Task<IActionResult> Index(int? courseId, int page = 1, CancellationToken ct = default)
    {
        ViewBag.CourseId = courseId;

        if (courseId.HasValue && courseId.Value > 0)
        {
            var result = await discussionService.GetByCourseAsync(courseId.Value, page, DisplayConstants.DiscussionPageSize, ct);
            return View(result);
        }

        // 全站討論列表
        var all = await discussionService.GetAllAsync(page, DisplayConstants.DiscussionPageSize, ct);
        return View(all);
    }

    /// <summary>討論詳情頁，顯示主題內容與所有回覆；找不到時回傳 404</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await discussionService.GetDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>新增討論表單頁（GET），未帶 courseId 時先顯示課程選擇頁</summary>
    [Authorize]
    public async Task<IActionResult> Create(int? courseId, CancellationToken ct = default)
    {
        // 如果沒帶 courseId，讓用戶選擇課程
        if (!courseId.HasValue || courseId.Value <= 0)
        {
            var courses = await courseService.GetPublishedCoursesAsync(ct);
            ViewBag.Courses = courses;
            return View("SelectCourse");
        }

        ViewBag.CourseId = courseId.Value;
        return View();
    }

    /// <summary>新增討論（POST），發佈成功後導向討論詳情頁</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, string title, string content, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await discussionService.CreateAsync(userId, courseId, title, content, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "討論已發佈";
            return RedirectToAction(nameof(Detail), new { id = result.Data });
        }
        TempData[TempDataKeys.Error] = result.ErrorMessage;
        ViewBag.CourseId = courseId;
        return View();
    }

    /// <summary>回覆討論（POST），回覆後導回討論詳情頁</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int discussionId, string content, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await discussionService.ReplyAsync(userId, discussionId, content, ct);
        if (result.IsSuccess)
            TempData[TempDataKeys.Success] = "回覆已發佈";
        else
            TempData[TempDataKeys.Error] = result.ErrorMessage;
        return RedirectToAction(nameof(Detail), new { id = discussionId });
    }

    /// <summary>刪除討論（POST，軟刪除），作者或管理員可執行，完成後導回列表</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await discussionService.DeleteAsync(id, userId, isAdmin, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index), new { courseId });
    }
}
