using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

[Area("Learn")]
[Authorize]
public class FlashcardController(IFlashcardLearnService flashcardLearnService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var decks = await flashcardLearnService.GetDecksForStudyAsync(userId, ct);
        return View(decks);
    }

    public async Task<IActionResult> Study(int id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var vm = await flashcardLearnService.GetStudySessionAsync(id, userId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Review(
        [FromBody] ReviewRequest request, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await flashcardLearnService.ReviewCardAsync(userId, request.CardId, request.Quality, ct);
        return Json(new { success = result.IsSuccess });
    }
}

public class ReviewRequest
{
    public int CardId { get; set; }
    public int Quality { get; set; }
}
