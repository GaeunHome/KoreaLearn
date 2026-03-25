using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Controllers;

[Authorize]
public class CertificateController(ICertificateService certificateService) : Controller
{
    public async Task<IActionResult> Check(int courseId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var eligibility = await certificateService.CheckEligibilityAsync(userId, courseId, ct);
        ViewBag.CourseId = courseId;
        return View(eligibility);
    }

    public async Task<IActionResult> Download(int courseId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var pdf = await certificateService.GenerateCertificateAsync(userId, courseId, ct);
        if (pdf is null)
        {
            TempData["Error"] = "您尚未滿足證書取得條件";
            return RedirectToAction(nameof(Check), new { courseId });
        }
        return File(pdf, "application/pdf", $"certificate-course-{courseId}.pdf");
    }
}
