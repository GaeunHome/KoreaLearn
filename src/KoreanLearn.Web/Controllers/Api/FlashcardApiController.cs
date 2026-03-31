using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Controllers.Api;

/// <summary>字卡複習 API，提供 SM-2 演算法複習結果提交</summary>
[Route("api/flashcard")]
[Authorize]
public class FlashcardApiController(IFlashcardLearnService flashcardService) : BaseApiController
{
    /// <summary>提交字卡複習結果</summary>
    [HttpPost("review")]
    public async Task<IActionResult> Review([FromBody] ReviewRequest request, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await flashcardService.ReviewCardAsync(userId, request.CardId, request.Quality, ct);
        return Ok(new { success = result.IsSuccess });
    }

    /// <summary>取得待複習字卡數量</summary>
    [HttpGet("due-count")]
    public async Task<IActionResult> GetDueCount(CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var count = await flashcardService.GetDueCardCountAsync(userId, ct);
        return Ok(new { count });
    }
}
