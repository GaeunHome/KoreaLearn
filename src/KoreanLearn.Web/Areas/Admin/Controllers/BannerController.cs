using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Banner;
using KoreanLearn.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台幻燈片管理 Controller</summary>
public class BannerController(IBannerService bannerService, IFileUploadService fileUploadService, ILogger<BannerController> logger) : AdminBaseController
{
    /// <summary>幻燈片列表</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看幻燈片列表 | UserId={UserId}", GetCurrentUserId());
        var banners = await bannerService.GetAllBannersAsync(ct);
        return View(banners);
    }

    /// <summary>新增表單頁面</summary>
    public IActionResult Create()
    {
        logger.LogInformation("管理員進入新增幻燈片頁面 | UserId={UserId}", GetCurrentUserId());
        return View(new BannerFormViewModel());
    }

    /// <summary>新增幻燈片</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BannerFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增幻燈片失敗：模型驗證錯誤 | UserId={UserId}", GetCurrentUserId());
            return View(vm);
        }

        if (vm.ImageFile is not null)
            vm.ImageUrl = await fileUploadService.SaveAsync(vm.ImageFile, "banners", "banner");

        var result = await bannerService.CreateAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員新增幻燈片成功 | UserId={UserId}", GetCurrentUserId());
            TempData[TempDataKeys.Success] = "幻燈片建立成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("管理員新增幻燈片失敗 | Error={Error} | UserId={UserId}", result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯表單頁面</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯幻燈片頁面 | BannerId={BannerId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await bannerService.GetForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看幻燈片失敗：資料不存在 | BannerId={BannerId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新幻燈片</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BannerFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新幻燈片失敗：模型驗證錯誤 | BannerId={BannerId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            return View(vm);
        }

        if (vm.ImageFile is not null)
            vm.ImageUrl = await fileUploadService.SaveAsync(vm.ImageFile, "banners", "banner");

        var result = await bannerService.UpdateAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新幻燈片成功 | BannerId={BannerId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "幻燈片更新成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("管理員更新幻燈片失敗 | BannerId={BannerId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除幻燈片（軟刪除）</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await bannerService.DeleteAsync(id, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員刪除幻燈片 | BannerId={BannerId} | UserId={UserId}", id, GetCurrentUserId());
        else
            logger.LogWarning("管理員刪除幻燈片失敗 | BannerId={BannerId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "幻燈片已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>切換幻燈片啟用/停用狀態</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct = default)
    {
        var result = await bannerService.ToggleActiveAsync(id, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員切換幻燈片狀態 | BannerId={BannerId} | UserId={UserId}", id, GetCurrentUserId());
        else
            logger.LogWarning("管理員切換幻燈片狀態失敗 | BannerId={BannerId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "狀態已切換" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Index));
    }

}
