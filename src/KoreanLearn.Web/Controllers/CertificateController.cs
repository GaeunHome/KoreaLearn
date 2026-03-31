using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure;

namespace KoreanLearn.Web.Controllers;

/// <summary>證書 Controller，提供完課資格檢查與 PDF 證書下載</summary>
[Authorize]
public class CertificateController(ICertificateService certificateService) : BaseController
{
    /// <summary>檢查完課資格，顯示各項條件（課程完成度、測驗分數）是否達標</summary>
    public async Task<IActionResult> Check(int courseId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var eligibility = await certificateService.CheckEligibilityAsync(userId, courseId, ct);
        ViewBag.CourseId = courseId;
        return View(eligibility);
    }

    /// <summary>下載 PDF 證書，未滿足條件時導回資格檢查頁</summary>
    public async Task<IActionResult> Download(int courseId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var pdf = await certificateService.GenerateCertificateAsync(userId, courseId, ct);
        if (pdf is null)
        {
            TempData[TempDataKeys.Error] = "您尚未滿足證書取得條件";
            return RedirectToAction(nameof(Check), new { courseId });
        }
        return File(pdf, "application/pdf", $"certificate-course-{courseId}.pdf");
    }
}
