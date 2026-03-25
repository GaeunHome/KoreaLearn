using KoreanLearn.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers;

public class CourseController(ICourseService courseService) : Controller
{
    public async Task<IActionResult> Index(
        string? keyword, int page = 1, CancellationToken ct = default)
    {
        var result = await courseService.SearchCoursesAsync(keyword, page, pageSize: 12, ct);
        ViewBag.Keyword = keyword;
        return View(result);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var course = await courseService.GetCourseDetailAsync(id, ct);
        if (course is null) return NotFound();
        return View(course);
    }
}
