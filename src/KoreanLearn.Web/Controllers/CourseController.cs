using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

/// <summary>課程公開頁面 Controller，提供課程列表搜尋與課程詳情瀏覽（未登入亦可存取）</summary>
public class CourseController(
    ICourseService courseService,
    ILogger<CourseController> logger) : BaseController
{
    /// <summary>課程列表頁（支援關鍵字搜尋與分頁），回傳分頁結果</summary>
    public async Task<IActionResult> Index(
        string? keyword, int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("瀏覽課程列表 | Keyword={Keyword} | Page={Page} | UserId={UserId}",
            keyword, page, GetCurrentUserId());
        var result = await courseService.SearchCoursesAsync(keyword, page, pageSize: DisplayConstants.CoursePageSize, ct);
        ViewBag.Keyword = keyword;
        return View(result);
    }

    /// <summary>課程詳情頁，顯示課程資訊、章節結構與購買狀態；找不到時回傳 404</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("查看課程詳情 | CourseId={CourseId} | UserId={UserId}",
            id, GetCurrentUserId());
        var userId = GetCurrentUserId();
        var course = await courseService.GetCourseDetailAsync(id, userId, ct);
        if (course is null)
        {
            logger.LogWarning("課程不存在或未發佈 | CourseId={CourseId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(course);
    }
}
