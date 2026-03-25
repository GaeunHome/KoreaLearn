using System.Security.Claims;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

/// <summary>課程公開頁面 Controller，提供課程列表搜尋與課程詳情瀏覽（未登入亦可存取）</summary>
public class CourseController(
    ICourseService courseService,
    ILogger<CourseController> logger) : Controller
{
    /// <summary>課程列表頁（支援關鍵字搜尋與分頁），回傳分頁結果</summary>
    public async Task<IActionResult> Index(
        string? keyword, int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("課程列表 | Keyword={Keyword} | Page={Page} | User={User}",
            keyword ?? "(空)", page, User.Identity?.Name ?? "Anonymous");
        var result = await courseService.SearchCoursesAsync(keyword, page, pageSize: 12, ct);
        ViewBag.Keyword = keyword;
        return View(result);
    }

    /// <summary>課程詳情頁，顯示課程資訊、章節結構與購買狀態；找不到時回傳 404</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("課程詳情 | CourseId={CourseId} | User={User}",
            id, User.Identity?.Name ?? "Anonymous");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var course = await courseService.GetCourseDetailAsync(id, userId, ct);
        if (course is null)
        {
            logger.LogWarning("課程不存在或未發佈 | CourseId={CourseId}", id);
            return NotFound();
        }
        return View(course);
    }
}
