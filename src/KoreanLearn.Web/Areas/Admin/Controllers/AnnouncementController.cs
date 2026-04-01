using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Announcement;
using KoreanLearn.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Web.Areas.Admin.Controllers;

/// <summary>後台公告管理 Controller</summary>
public class AnnouncementController(
    IAnnouncementService announcementService,
    IFileUploadService fileUploadService,
    ILogger<AnnouncementController> logger) : AdminBaseController
{
    /// <summary>公告列表（含軟刪除）</summary>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        logger.LogInformation("管理員查看公告列表 | UserId={UserId}", GetCurrentUserId());
        var items = await announcementService.GetAllForAdminAsync(ct);
        return View(items);
    }

    /// <summary>新增公告頁面</summary>
    public IActionResult Create()
    {
        logger.LogInformation("管理員進入新增公告頁面 | UserId={UserId}", GetCurrentUserId());
        return View(new AnnouncementFormViewModel());
    }

    /// <summary>新增公告</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AnnouncementFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return View(vm);

        var attachments = await UploadAttachmentsAsync(vm.AttachmentFiles);
        var result = await announcementService.CreateAsync(vm, attachments, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員新增公告成功 | AnnouncementId={AnnouncementId} | UserId={UserId}",
                result.Data, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "公告建立成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("管理員新增公告失敗 | Error={Error} | UserId={UserId}", result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "建立失敗");
        return View(vm);
    }

    /// <summary>編輯公告頁面</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        logger.LogInformation("管理員進入編輯公告頁面 | AnnouncementId={AnnouncementId} | UserId={UserId}",
            id, GetCurrentUserId());
        var vm = await announcementService.GetForEditAsync(id, ct);
        if (vm is null) return NotFound();
        return View(vm);
    }

    /// <summary>更新公告</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AnnouncementFormViewModel vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            // 需重新載入既有附件
            var existing = await announcementService.GetForEditAsync(vm.Id, ct);
            if (existing is not null) vm.ExistingAttachments = existing.ExistingAttachments;
            return View(vm);
        }

        var newAttachments = await UploadAttachmentsAsync(vm.AttachmentFiles);
        var result = await announcementService.UpdateAsync(vm, newAttachments, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("管理員更新公告成功 | AnnouncementId={AnnouncementId} | UserId={UserId}",
                vm.Id, GetCurrentUserId());
            TempData[TempDataKeys.Success] = "公告更新成功";
            return RedirectToAction(nameof(Index));
        }

        logger.LogWarning("管理員更新公告失敗 | AnnouncementId={AnnouncementId} | Error={Error} | UserId={UserId}",
            vm.Id, result.ErrorMessage, GetCurrentUserId());
        ModelState.AddModelError("", result.ErrorMessage ?? "更新失敗");
        return View(vm);
    }

    /// <summary>軟刪除公告</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await announcementService.SoftDeleteAsync(id, ct);
        logger.LogInformation("管理員刪除公告 | AnnouncementId={AnnouncementId} | Success={Success} | UserId={UserId}",
            id, result.IsSuccess, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "公告已刪除" : (result.ErrorMessage ?? "刪除失敗");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>復原軟刪除的公告</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken ct = default)
    {
        var result = await announcementService.RestoreAsync(id, ct);
        logger.LogInformation("管理員復原公告 | AnnouncementId={AnnouncementId} | Success={Success} | UserId={UserId}",
            id, result.IsSuccess, GetCurrentUserId());
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "公告已復原" : (result.ErrorMessage ?? "復原失敗");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>切換置頂狀態</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePin(int id, CancellationToken ct = default)
    {
        var result = await announcementService.TogglePinAsync(id, ct);
        TempData[result.IsSuccess ? TempDataKeys.Success : TempDataKeys.Error] =
            result.IsSuccess ? "置頂狀態已切換" : (result.ErrorMessage ?? "操作失敗");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>拖曳排序（AJAX）</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds, CancellationToken ct = default)
    {
        var result = await announcementService.ReorderAsync(orderedIds, ct);
        return Json(new { success = result.IsSuccess });
    }

    /// <summary>上傳多個附件檔案</summary>
    private async Task<IReadOnlyList<(string fileUrl, string fileName, long fileSize)>?> UploadAttachmentsAsync(
        IReadOnlyList<IFormFile>? files)
    {
        if (files is not { Count: > 0 }) return null;

        var result = new List<(string, string, long)>();
        foreach (var file in files)
        {
            var url = await fileUploadService.SaveAsync(file, "announcements", "ann");
            result.Add((url, file.FileName, file.Length));
        }
        return result;
    }
}
