using System.Security.Claims;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Web.Controllers.Api;

/// <summary>學習進度 API，提供影片進度儲存與單元完成標記</summary>
[Route("api/progress")]
[Authorize]
public class ProgressApiController(
    IProgressService progressService,
    ILogger<ProgressApiController> logger) : BaseApiController
{
    /// <summary>儲存影片觀看進度</summary>
    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressRequest request, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("API：儲存影片進度 | LessonId={LessonId} | ProgressSeconds={ProgressSeconds} | UserId={UserId}",
            request.LessonId, request.ProgressSeconds, userId);
        var result = await progressService.SaveVideoProgressAsync(userId, request.LessonId, request.ProgressSeconds, GetUserRoles(), ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("API：儲存影片進度成功 | LessonId={LessonId} | ProgressSeconds={ProgressSeconds} | UserId={UserId}",
                request.LessonId, result.Data, userId);
            return Ok(new { success = true, progressSeconds = result.Data });
        }
        logger.LogWarning("API：儲存影片進度失敗 | LessonId={LessonId} | Error={Error} | UserId={UserId}",
            request.LessonId, result.ErrorMessage, userId);
        return BadRequest(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>標記單元完成</summary>
    [HttpPost("complete/{lessonId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int lessonId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("API：標記單元完成 | LessonId={LessonId} | UserId={UserId}", lessonId, userId);
        var result = await progressService.MarkLessonCompleteAsync(userId, lessonId, GetUserRoles(), ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("API：標記單元完成成功 | LessonId={LessonId} | UserId={UserId}", lessonId, userId);
            return Ok(new { success = true });
        }
        logger.LogWarning("API：標記單元完成失敗 | LessonId={LessonId} | Error={Error} | UserId={UserId}",
            lessonId, result.ErrorMessage, userId);
        return BadRequest(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>取消單元完成</summary>
    [HttpPost("undo-complete/{lessonId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UndoComplete(int lessonId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("API：取消單元完成 | LessonId={LessonId} | UserId={UserId}", lessonId, userId);
        var result = await progressService.UndoLessonCompleteAsync(userId, lessonId, GetUserRoles(), ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("API：取消單元完成成功 | LessonId={LessonId} | UserId={UserId}", lessonId, userId);
            return Ok(new { success = true });
        }
        logger.LogWarning("API：取消單元完成失敗 | LessonId={LessonId} | Error={Error} | UserId={UserId}",
            lessonId, result.ErrorMessage, userId);
        return BadRequest(new { success = false, error = result.ErrorMessage });
    }

    private IEnumerable<string> GetUserRoles()
        => User.FindAll(ClaimTypes.Role).Select(c => c.Value);
}
