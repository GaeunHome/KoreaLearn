using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Flashcard;
using KoreanLearn.Web.Infrastructure;
using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台字卡牌組管理 Controller，提供牌組與字卡的 CRUD 操作</summary>
public class FlashcardDeckController(IFlashcardAdminService flashcardAdminService) : AdminBaseController
{
    /// <summary>牌組列表頁（分頁），顯示所有字卡牌組</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.GetDecksPagedAsync(page, DisplayConstants.AdminPageSize, ct);
        return View(result);
    }

    /// <summary>新增牌組表單頁（GET）</summary>
    public IActionResult Create() => View(new DeckFormViewModel());

    /// <summary>新增牌組（POST），成功後導向牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeckFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.CreateDeckAsync(vm, ct);
        if (result is { IsSuccess: true, Data: var deckId })
        {
            TempData[TempDataKeys.Success] = "牌組建立成功";
            return RedirectToAction(nameof(Detail), new { id = deckId });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>牌組詳情頁，顯示牌組資訊與所有字卡</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await flashcardAdminService.GetDeckDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>編輯牌組表單頁（GET），載入現有牌組資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await flashcardAdminService.GetDeckForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新牌組（POST），成功後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DeckFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.UpdateDeckAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "牌組更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除牌組（POST，軟刪除），完成後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.DeleteDeckAsync(id, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "牌組已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    // ── 字卡管理 ──

    /// <summary>新增字卡表單頁（GET），預帶所屬牌組資訊</summary>
    public IActionResult AddCard(int deckId, string? deckTitle)
    {
        return View(new CardFormViewModel { DeckId = deckId, DeckTitle = deckTitle });
    }

    /// <summary>新增字卡（POST），成功後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCard(CardFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.AddCardAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "字卡新增成功";
            return RedirectToAction(nameof(Detail), new { id = vm.DeckId });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "新增失敗");
        return View(vm);
    }

    /// <summary>編輯字卡表單頁（GET），載入現有字卡資料</summary>
    public async Task<IActionResult> EditCard(int id, CancellationToken ct = default)
    {
        var vm = await flashcardAdminService.GetCardForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新字卡（POST），成功後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCard(CardFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.UpdateCardAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "字卡更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.DeckId });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除字卡（POST），完成後導回牌組詳情頁</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCard(int id, int deckId, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.DeleteCardAsync(id, deckId, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] = result.IsSuccess ? "字卡已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Detail), new { id = deckId });
    }
}
