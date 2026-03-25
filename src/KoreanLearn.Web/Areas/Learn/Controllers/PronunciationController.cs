using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

[Area("Learn")]
[Authorize]
public class PronunciationController(IPronunciationService pronunciationService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var exercises = await pronunciationService.GetAllForPracticeAsync(ct);
        return View(exercises);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(int exerciseId, IFormFile recording, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "recordings");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(recording.FileName)}";
        var path = Path.Combine(dir, fileName);
        await using var stream = new FileStream(path, FileMode.Create);
        await recording.CopyToAsync(stream, ct);

        var recordingUrl = $"/uploads/recordings/{fileName}";
        var result = await pronunciationService.SaveAttemptAsync(userId, exerciseId, recordingUrl, ct);

        return Json(new { success = result.IsSuccess, recordingUrl });
    }
}
