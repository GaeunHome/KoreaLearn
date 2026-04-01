using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Flashcard;
using KoreanLearn.Web.Infrastructure;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台字卡牌組管理 Controller，提供牌組與字卡的 CRUD 操作</summary>
public class FlashcardDeckController(IFlashcardAdminService flashcardAdminService, ILogger<FlashcardDeckController> logger) : AdminBaseController
{
    /// <summary>牌組列表頁（分頁），顯示所有字卡牌組</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看牌組列表 | Page={Page} | UserId={UserId}", page, GetCurrentUserId());
        var result = await flashcardAdminService.GetDecksPagedAsync(page, DisplayConstants.AdminPageSize, ct);
        return View(result);
    }

    /// <summary>新增牌組表單頁（GET）</summary>
    public IActionResult Create()
    {
        logger.LogInformation("管理員進入新增牌組頁面 | UserId={UserId}", GetCurrentUserId());
        return View(new DeckFormViewModel());
    }

    /// <summary>新增牌組（POST），成功後導向牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeckFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增牌組失敗：模型驗證錯誤 | UserId={UserId}", GetCurrentUserId());
            return View(vm);
        }
        var result = await flashcardAdminService.CreateDeckAsync(vm, ct);
        if (result is { IsSuccess: true, Data: var deckId })
        {
            logger.LogInformation("管理員新增牌組成功 | DeckId={DeckId} | UserId={UserId}", deckId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "牌組建立成功";
            return RedirectToAction(nameof(Detail), new { id = deckId });
        }
        logger.LogWarning("管理員新增牌組失敗 | Error={Error} | UserId={UserId}", result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>牌組詳情頁，顯示牌組資訊與所有字卡</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看牌組詳情 | DeckId={DeckId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await flashcardAdminService.GetDeckDetailAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看牌組失敗：資料不存在 | DeckId={DeckId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>編輯牌組表單頁（GET），載入現有牌組資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯牌組頁面 | DeckId={DeckId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await flashcardAdminService.GetDeckForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看牌組失敗：資料不存在 | DeckId={DeckId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新牌組（POST），成功後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DeckFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新牌組失敗：模型驗證錯誤 | DeckId={DeckId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            return View(vm);
        }
        var result = await flashcardAdminService.UpdateDeckAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新牌組成功 | DeckId={DeckId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "牌組更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }
        logger.LogWarning("管理員更新牌組失敗 | DeckId={DeckId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除牌組（POST，軟刪除），完成後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.DeleteDeckAsync(id, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員刪除牌組 | DeckId={DeckId} | UserId={UserId}", id, GetCurrentUserId());
        else
            logger.LogWarning("管理員刪除牌組失敗 | DeckId={DeckId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "牌組已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    // ── 字卡管理 ──

    /// <summary>新增字卡表單頁（GET），預帶所屬牌組資訊</summary>
    public IActionResult AddCard(int deckId, string? deckTitle)
    {
        logger.LogInformation("管理員進入新增字卡頁面 | DeckId={DeckId} | UserId={UserId}", deckId, GetCurrentUserId());
        return View(new CardFormViewModel { DeckId = deckId, DeckTitle = deckTitle });
    }

    /// <summary>新增字卡（POST），成功後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCard(CardFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增字卡失敗：模型驗證錯誤 | DeckId={DeckId} | UserId={UserId}", vm.DeckId, GetCurrentUserId());
            return View(vm);
        }
        var result = await flashcardAdminService.AddCardAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員新增字卡成功 | DeckId={DeckId} | UserId={UserId}", vm.DeckId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "字卡新增成功";
            return RedirectToAction(nameof(Detail), new { id = vm.DeckId });
        }
        logger.LogWarning("管理員新增字卡失敗 | DeckId={DeckId} | Error={Error} | UserId={UserId}", vm.DeckId, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "新增失敗");
        return View(vm);
    }

    /// <summary>編輯字卡表單頁（GET），載入現有字卡資料</summary>
    public async Task<IActionResult> EditCard(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯字卡頁面 | CardId={CardId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await flashcardAdminService.GetCardForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看字卡失敗：資料不存在 | CardId={CardId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新字卡（POST），成功後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCard(CardFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新字卡失敗：模型驗證錯誤 | CardId={CardId} | DeckId={DeckId} | UserId={UserId}", vm.Id, vm.DeckId, GetCurrentUserId());
            return View(vm);
        }
        var result = await flashcardAdminService.UpdateCardAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新字卡成功 | CardId={CardId} | DeckId={DeckId} | UserId={UserId}", vm.Id, vm.DeckId, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "字卡更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.DeckId });
        }
        logger.LogWarning("管理員更新字卡失敗 | CardId={CardId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除字卡（POST），完成後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCard(int id, int deckId, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.DeleteCardAsync(id, deckId, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員刪除字卡 | CardId={CardId} | DeckId={DeckId} | UserId={UserId}", id, deckId, GetCurrentUserId());
        else
            logger.LogWarning("管理員刪除字卡失敗 | CardId={CardId} | DeckId={DeckId} | Error={Error} | UserId={UserId}", id, deckId, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "字卡已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Detail), new { id = deckId });
    }
}
