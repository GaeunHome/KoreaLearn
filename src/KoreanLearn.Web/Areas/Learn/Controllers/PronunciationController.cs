using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>發音練習 Controller，提供發音練習列表與學生錄音上傳功能</summary>
public class PronunciationController(
    IPronunciationService pronunciationService,
    IFileUploadService fileUploadService,
    ILogger<PronunciationController> logger) : LearnBaseController
{
    /// <summary>發音練習列表頁，顯示所有可練習的發音題目與標準音檔</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        logger.LogInformation("學生查看發音練習列表 | UserId={UserId}", GetAuthorizedUserId());
        var exercises = await pronunciationService.GetAllForPracticeAsync(ct);
        return View(exercises);
    }

    /// <summary>上傳學生錄音（POST），儲存錄音檔並記錄練習紀錄，回傳 JSON 結果</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int exerciseId, IFormFile recording, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();

        var recordingUrl = await fileUploadService.SaveAsync(recording, "recordings");
        var result = await pronunciationService.SaveAttemptAsync(userId, exerciseId, recordingUrl, ct);

        if (result.IsSuccess)
            logger.LogInformation("學生上傳發音錄音成功 | ExerciseId={ExerciseId} | UserId={UserId}", exerciseId, userId);
        else
            logger.LogWarning("學生上傳發音錄音失敗 | ExerciseId={ExerciseId} | Error={Error} | UserId={UserId}", exerciseId, result.ErrorMessage, userId);

        return Json(new { success = result.IsSuccess, recordingUrl });
    }
}
