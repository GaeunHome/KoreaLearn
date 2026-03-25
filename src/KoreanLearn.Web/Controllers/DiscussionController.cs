using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Controllers;

public class DiscussionController(IDiscussionService discussionService) : Controller
{
    public async Task<IActionResult> Index(int courseId, int page = 1, CancellationToken ct = default)
    {
        var result = await discussionService.GetByCourseAsync(courseId, page, 20, ct);
        ViewBag.CourseId = courseId;
        return View(result);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await discussionService.GetDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [Authorize]
    public IActionResult Create(int courseId)
    {
        ViewBag.CourseId = courseId;
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, string title, string content, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await discussionService.CreateAsync(userId, courseId, title, content, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "討論已發佈";
            return RedirectToAction(nameof(Detail), new { id = result.Data });
        }
        TempData["Error"] = result.ErrorMessage;
        ViewBag.CourseId = courseId;
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int discussionId, string content, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await discussionService.ReplyAsync(userId, discussionId, content, ct);
        if (result.IsSuccess)
            TempData["Success"] = "回覆已發佈";
        else
            TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Detail), new { id = discussionId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        var result = await discussionService.DeleteAsync(id, userId, isAdmin, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index), new { courseId });
    }
}
