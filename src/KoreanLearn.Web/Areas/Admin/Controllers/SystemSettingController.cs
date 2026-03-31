using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.SystemSetting;
using KoreanLearn.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台系統參數管理 Controller</summary>
public class SystemSettingController(ISystemSettingService settingService) : AdminBaseController
{
    /// <summary>系統參數列表</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var settings = await settingService.GetAllSettingsAsync(ct);
        return View(settings);
    }

    /// <summary>新增表單頁面</summary>
    public IActionResult Create() => View(new SystemSettingFormViewModel());

    /// <summary>新增系統參數</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SystemSettingFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await settingService.CreateAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "系統參數建立成功";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯表單頁面</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var vm = await settingService.GetForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新系統參數</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SystemSettingFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await settingService.UpdateAsync(vm, ct);
        if (result.IsSuccess)
        {
            TempData[TempDataKeys.Success] = "系統參數更新成功";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>刪除系統參數</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await settingService.DeleteAsync(id, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }
}
