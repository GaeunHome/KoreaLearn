using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

/// <summary>教師儀表板 Controller，顯示教師的課程統計與管理摘要</summary>
public class DashboardController(ITeacherCourseService teacherService) : TeacherBaseController
{
    /// <summary>教師儀表板首頁，顯示該教師名下課程的統計數據</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var vm = await teacherService.GetDashboardAsync(TeacherId, ct);
        return View(vm);
    }
}
