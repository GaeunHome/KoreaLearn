using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.SystemSetting;
using KoreanLearn.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台系統參數管理 Controller</summary>
public class SystemSettingController(ISystemSettingService settingService, ILogger<SystemSettingController> logger) : AdminBaseController
{
    /// <summary>系統參數列表</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看系統參數列表 | UserId={UserId}", GetCurrentUserId());
        var settings = await settingService.GetAllSettingsAsync(ct);
        return View(settings);
    }

    /// <summary>新增表單頁面</summary>
    public IActionResult Create()
    {
        logger.LogInformation("管理員進入新增系統參數頁面 | UserId={UserId}", GetCurrentUserId());
        return View(new SystemSettingFormViewModel());
    }

    /// <summary>新增系統參數</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SystemSettingFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員新增系統參數失敗：模型驗證錯誤 | UserId={UserId}", GetCurrentUserId());
            return View(vm);
        }

        var result = await settingService.CreateAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員新增系統參數成功 | UserId={UserId}", GetCurrentUserId());
            TempData[TempDataKeys.Success] = "系統參數建立成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("管理員新增系統參數失敗 | Error={Error} | UserId={UserId}", result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯表單頁面</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯系統參數頁面 | SettingId={SettingId} | UserId={UserId}", id, GetCurrentUserId());
        var vm = await settingService.GetForEditAsync(id, ct);
        if (vm is null)
        {
            logger.LogWarning("管理員查看系統參數失敗：資料不存在 | SettingId={SettingId} | UserId={UserId}", id, GetCurrentUserId());
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>更新系統參數</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SystemSettingFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("管理員更新系統參數失敗：模型驗證錯誤 | SettingId={SettingId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            return View(vm);
        }

        var result = await settingService.UpdateAsync(vm, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新系統參數成功 | SettingId={SettingId} | UserId={UserId}", vm.Id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "系統參數更新成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("管理員更新系統參數失敗 | SettingId={SettingId} | Error={Error} | UserId={UserId}", vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除系統參數</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await settingService.DeleteAsync(id, ct);
        if (result.IsSuccess)
            logger.LogInformation("管理員刪除系統參數 | SettingId={SettingId} | UserId={UserId}", id, GetCurrentUserId());
        else
            logger.LogWarning("管理員刪除系統參數失敗 | SettingId={SettingId} | Error={Error} | UserId={UserId}", id, result.ErrorMessage, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }
}
