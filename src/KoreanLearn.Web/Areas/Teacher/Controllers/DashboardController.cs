using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class DashboardController(ITeacherCourseService teacherService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await teacherService.GetDashboardAsync(teacherId, ct);
        return View(vm);
    }
}
