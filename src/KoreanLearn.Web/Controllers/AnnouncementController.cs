using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Web.Controllers;

/// <summary>前台公告 Controller，提供公告列表與詳情頁</summary>
public class AnnouncementController(
    IAnnouncementService announcementService,
    ILogger<AnnouncementController> logger) : BaseController
{
    private const int PageSize = 10;

    /// <summary>公告列表（分頁）</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("瀏覽公告列表 | Page={Page} | UserId={UserId}", page, GetCurrentUserId());
        var result = await announcementService.GetPublishedPagedAsync(page, PageSize, ct);

        ViewBag.CurrentPage = result.Page;
        ViewBag.TotalPages = result.TotalPages;
        ViewBag.HasPreviousPage = result.HasPrevious;
        ViewBag.HasNextPage = result.HasNext;

        return View(result);
    }

    /// <summary>公告詳情</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("查看公告詳情 | AnnouncementId={AnnouncementId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await announcementService.GetDetailAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("公告不存在 | AnnouncementId={AnnouncementId}", id);
            return NotFound();
        }
        return View(vm);
    }
}
