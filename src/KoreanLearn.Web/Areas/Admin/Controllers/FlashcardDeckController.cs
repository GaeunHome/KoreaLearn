using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Flashcard;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FlashcardDeckController(IFlashcardAdminService flashcardAdminService) : Controller
{
    private const int PageSize = 20;

    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.GetDecksPagedAsync(page, PageSize, ct);
        return View(result);
    }

    public IActionResult Create() => View(new DeckFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeckFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.CreateDeckAsync(vm, ct);
        if (result is { IsSuccess: true, Data: var deckId })
        {
            TempData["Success"] = "牌組建立成功";
            return RedirectToAction(nameof(Detail), new { id = deckId });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var vm = await flashcardAdminService.GetDeckDetailAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await flashcardAdminService.GetDeckForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DeckFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.UpdateDeckAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "牌組更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.DeleteDeckAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "牌組已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    // ── Card ──

    public IActionResult AddCard(int deckId, string? deckTitle)
    {
        return View(new CardFormViewModel { DeckId = deckId, DeckTitle = deckTitle });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCard(CardFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.AddCardAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "字卡新增成功";
            return RedirectToAction(nameof(Detail), new { id = vm.DeckId });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "新增失敗");
        return View(vm);
    }

    public async Task<IActionResult> EditCard(int id, CancellationToken ct = default)
    {
        var vm = await flashcardAdminService.GetCardForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCard(CardFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await flashcardAdminService.UpdateCardAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "字卡更新成功";
            return RedirectToAction(nameof(Detail), new { id = vm.DeckId });
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCard(int id, int deckId, CancellationToken ct = default)
    {
        var result = await flashcardAdminService.DeleteCardAsync(id, deckId, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "字卡已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Detail), new { id = deckId });
    }
}
