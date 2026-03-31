using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Learn;

namespace KoreanLearn.Web.Areas.Learn.Controllers;

/// <summary>字卡學習 Controller，提供字卡牌組列表、學習介面與 SM-2 間隔重複複習</summary>
public class FlashcardController(IFlashcardLearnService flashcardLearnService) : LearnBaseController
{
    /// <summary>字卡牌組列表頁，顯示所有可學習的牌組與待複習數量</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var decks = await flashcardLearnService.GetDecksForStudyAsync(userId, ct);
        return View(decks);
    }

    /// <summary>字卡學習介面，以翻轉動畫逐張顯示字卡（韓文/中文/羅馬拼音）</summary>
    public async Task<IActionResult> Study(int id, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var vm = await flashcardLearnService.GetStudySessionAsync(id, userId, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>提交字卡複習結果（POST，JSON），依 SM-2 演算法更新下次複習時間</summary>
    [HttpPost]
    public async Task<IActionResult> Review(
        [FromBody] ReviewRequest request, CancellationToken ct = default)
    {
        var userId = GetAuthorizedUserId();
        var result = await flashcardLearnService.ReviewCardAsync(userId, request.CardId, request.Quality, ct);
        return Json(new { success = result.IsSuccess });
    }
}
