using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Pronunciation;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台發音練習管理 Controller，提供發音練習題的 CRUD 操作（含標準音檔上傳）</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PronunciationController(IPronunciationService pronunciationService) : Controller
{
    /// <summary>發音練習列表頁（分頁），顯示所有發音練習題</summary>
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await pronunciationService.GetPagedAsync(page, 20, ct);
        return View(result);
    }

    /// <summary>新增發音練習表單頁（GET）</summary>
    public IActionResult Create() => View(new PronunciationFormViewModel());

    /// <summary>新增發音練習（POST），上傳標準音檔後建立練習題，成功導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PronunciationFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var audioUrl = vm.AudioFile is not null ? await SaveAudioAsync(vm.AudioFile) : "";
        if (string.IsNullOrEmpty(audioUrl) && vm.AudioFile is null)
        {
            ModelState.AddModelError("AudioFile", "請上傳標準音檔");
            return View(vm);
        }

        var result = await pronunciationService.CreateAsync(vm, audioUrl, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "發音練習建立成功";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯發音練習表單頁（GET），載入現有練習資料</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await pronunciationService.GetForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新發音練習（POST），可選擇更新標準音檔，成功導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PronunciationFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);
        string? newAudioUrl = vm.AudioFile is not null ? await SaveAudioAsync(vm.AudioFile) : null;
        var result = await pronunciationService.UpdateAsync(vm, newAudioUrl, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "發音練習更新成功";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除發音練習（POST，軟刪除），完成後導回列表</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await pronunciationService.DeleteAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>儲存音檔至 wwwroot/uploads/audio/，回傳相對路徑</summary>
    private static async Task<string> SaveAudioAsync(IFormFile file)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "audio");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(dir, fileName);
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/audio/{fileName}";
    }
}
