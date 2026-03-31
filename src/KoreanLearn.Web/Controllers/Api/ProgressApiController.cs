using System.Security.Claims;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers.Api;

/// <summary>學習進度 API，提供影片進度儲存與單元完成標記</summary>
[Route("api/progress")]
[Authorize]
public class ProgressApiController(IProgressService progressService) : BaseApiController
{
    /// <summary>儲存影片觀看進度</summary>
    [HttpPost("save")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressRequest request, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await progressService.SaveVideoProgressAsync(userId, request.LessonId, request.ProgressSeconds, GetUserRoles(), ct);
        return result.IsSuccess
            ? Ok(new { success = true, progressSeconds = result.Data })
            : BadRequest(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>標記單元完成</summary>
    [HttpPost("complete/{lessonId:int}")]
    public async Task<IActionResult> Complete(int lessonId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await progressService.MarkLessonCompleteAsync(userId, lessonId, GetUserRoles(), ct);
        return result.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.ErrorMessage });
    }

    /// <summary>取消單元完成</summary>
    [HttpPost("undo-complete/{lessonId:int}")]
    public async Task<IActionResult> UndoComplete(int lessonId, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await progressService.UndoLessonCompleteAsync(userId, lessonId, GetUserRoles(), ct);
        return result.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.ErrorMessage });
    }

    private IEnumerable<string> GetUserRoles()
        => User.FindAll(ClaimTypes.Role).Select(c => c.Value);
}
