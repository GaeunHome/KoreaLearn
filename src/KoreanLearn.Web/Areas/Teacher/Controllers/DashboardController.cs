using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師儀表板 Controller，顯示教師的課程統計與管理摘要</summary>
public class DashboardController(ITeacherCourseService teacherService, ILogger<DashboardController> logger) : TeacherBaseController
{
    /// <summary>教師儀表板首頁，顯示該教師名下課程的統計數據</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        logger.LogInformation("教師查看儀表板 | TeacherId={TeacherId}", TeacherId);
        var vm = await teacherService.GetDashboardAsync(TeacherId, ct);
        return View(vm);
    }
}
