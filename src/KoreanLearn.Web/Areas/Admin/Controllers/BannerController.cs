using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Banner;
using KoreanLearn.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台幻燈片管理 Controller</summary>
public class BannerController(IBannerService bannerService, IFileUploadService fileUploadService) : AdminBaseController
{
    /// <summary>幻燈片列表</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var banners = await bannerService.GetAllBannersAsync(ct);
        return View(banners);
    }

    /// <summary>新增表單頁面</summary>
    public IActionResult Create() => View(new BannerFormViewModel());

    /// <summary>新增幻燈片</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BannerFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.ImageFile is not null)
            vm.ImageUrl = await fileUploadService.SaveAsync(vm.ImageFile, "banners", "banner");

        var result = await bannerService.CreateAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "幻燈片建立成功";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯表單頁面</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await bannerService.GetForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新幻燈片</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BannerFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.ImageFile is not null)
            vm.ImageUrl = await fileUploadService.SaveAsync(vm.ImageFile, "banners", "banner");

        var result = await bannerService.UpdateAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "幻燈片更新成功";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除幻燈片（軟刪除）</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await bannerService.DeleteAsync(id, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "幻燈片已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>切換幻燈片啟用/停用狀態</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct = default)
    {
        var result = await bannerService.ToggleActiveAsync(id, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "狀態已切換" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Index));
    }

}
