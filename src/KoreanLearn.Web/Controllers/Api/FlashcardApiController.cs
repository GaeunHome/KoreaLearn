using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Web.Controllers.Api;

/// <summary>字卡複習 API，提供 SM-2 演算法複習結果提交</summary>
[Route("api/flashcard")]
[Authorize]
public class FlashcardApiController(
    IFlashcardLearnService flashcardService,
    ILogger<FlashcardApiController> logger) : BaseApiController
{
    /// <summary>提交字卡複習結果</summary>
    [HttpPost("review")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review([FromBody] ReviewRequest request, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("API：提交字卡複習結果 | CardId={CardId} | Quality={Quality} | UserId={UserId}",
            request.CardId, request.Quality, userId);
        var result = await flashcardService.ReviewCardAsync(userId, request.CardId, request.Quality, ct);
        if (result.IsSuccess)
            logger.LogInformation("API：提交字卡複習成功 | CardId={CardId} | UserId={UserId}",
                request.CardId, userId);
        else
            logger.LogWarning("API：提交字卡複習失敗 | CardId={CardId} | Error={Error} | UserId={UserId}",
                request.CardId, result.ErrorMessage, userId);
        return Ok(new { success = result.IsSuccess });
    }

    /// <summary>取得待複習字卡數量</summary>
    [HttpGet("due-count")]
    public async Task<IActionResult> GetDueCount(CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        logger.LogInformation("API：取得待複習字卡數量 | UserId={UserId}", userId);
        var count = await flashcardService.GetDueCardCountAsync(userId, ct);
        logger.LogInformation("API：取得待複習字卡數量成功 | Count={Count} | UserId={UserId}", count, userId);
        return Ok(new { count });
    }
}
