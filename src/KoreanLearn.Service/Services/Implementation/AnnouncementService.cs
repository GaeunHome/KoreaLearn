using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Announcement;
using KoreanLearn.Service.ViewModels.Announcement;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>公告業務邏輯實作</summary>
public class AnnouncementService(
    IUnitOfWork uow,
    ILogger<AnnouncementService> logger) : IAnnouncementService
{
    // ── 前台 ──

    /// <inheritdoc />
    public async Task<PagedResult<AnnouncementCardViewModel>> GetPublishedPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        logger.LogInformation("取得公告列表 | Page={Page} | PageSize={PageSize}", page, pageSize);
        var result = await uow.Announcements.GetPublishedPagedAsync(page, pageSize, ct).ConfigureAwait(false);
        var items = result.Items.Select(MapToCard).ToList();
        return new PagedResult<AnnouncementCardViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<AnnouncementDetailViewModel?> GetDetailAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("取得公告詳情 | AnnouncementId={AnnouncementId}", id);
        var announcement = await uow.Announcements.GetPublishedWithAttachmentsAsync(id, ct).ConfigureAwait(false);
        if (announcement is null)
        {
            logger.LogWarning("公告不存在或未發布 | AnnouncementId={AnnouncementId}", id);
            return null;
        }

        return new AnnouncementDetailViewModel
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            IsPinned = announcement.IsPinned,
            CreatedAt = announcement.CreatedAt,
            Attachments = announcement.Attachments.Select(MapAttachment).ToList()
        };
    }

    // ── 後台 ──

    /// <inheritdoc />
    public async Task<IReadOnlyList<AnnouncementListItemViewModel>> GetAllForAdminAsync(
        CancellationToken ct = default)
    {
        logger.LogInformation("後台取得公告列表");
        var announcements = await uow.Announcements.GetAllIncludeDeletedAsync(ct).ConfigureAwait(false);
        return announcements.Select(a => new AnnouncementListItemViewModel
        {
            Id = a.Id,
            Title = a.Title,
            IsActive = a.IsActive,
            IsPinned = a.IsPinned,
            SortOrder = a.SortOrder,
            IsDeleted = a.IsDeleted,
            CreatedAt = a.CreatedAt,
            AttachmentCount = a.Attachments.Count
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<AnnouncementFormViewModel?> GetForEditAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("後台取得公告編輯資料 | AnnouncementId={AnnouncementId}", id);
        var announcement = await uow.Announcements.GetWithAttachmentsIncludeDeletedAsync(id, ct).ConfigureAwait(false);
        if (announcement is null) return null;

        return new AnnouncementFormViewModel
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            IsActive = announcement.IsActive,
            IsPinned = announcement.IsPinned,
            StartDate = announcement.StartDate,
            EndDate = announcement.EndDate,
            SortOrder = announcement.SortOrder,
            ExistingAttachments = announcement.Attachments.Select(a => new ExistingAttachmentViewModel
            {
                Id = a.Id,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileSizeDisplay = FormatFileSize(a.FileSizeBytes)
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateAsync(
        AnnouncementFormViewModel vm,
        IReadOnlyList<(string fileUrl, string fileName, long fileSize)>? attachments,
        CancellationToken ct = default)
    {
        logger.LogInformation("建立公告 | Title={Title}", vm.Title);
        var maxSort = await uow.Announcements.GetMaxSortOrderAsync(ct).ConfigureAwait(false);

        var announcement = new Announcement
        {
            Title = vm.Title,
            Content = vm.Content,
            IsActive = vm.IsActive,
            IsPinned = vm.IsPinned,
            SortOrder = maxSort + 1,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate
        };

        if (attachments is { Count: > 0 })
        {
            for (var i = 0; i < attachments.Count; i++)
            {
                var (fileUrl, fileName, fileSize) = attachments[i];
                announcement.Attachments.Add(new AnnouncementAttachment
                {
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileSizeBytes = fileSize,
                    SortOrder = i
                });
            }
        }

        await uow.Announcements.AddAsync(announcement, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("公告建立成功 | AnnouncementId={AnnouncementId}", announcement.Id);
        return ServiceResult<int>.Success(announcement.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateAsync(
        AnnouncementFormViewModel vm,
        IReadOnlyList<(string fileUrl, string fileName, long fileSize)>? newAttachments,
        CancellationToken ct = default)
    {
        logger.LogInformation("更新公告 | AnnouncementId={AnnouncementId}", vm.Id);
        var announcement = await uow.Announcements.GetWithAttachmentsIncludeDeletedAsync(vm.Id, ct).ConfigureAwait(false);
        if (announcement is null)
        {
            logger.LogWarning("更新失敗：公告不存在 | AnnouncementId={AnnouncementId}", vm.Id);
            return ServiceResult.Failure("公告不存在");
        }

        announcement.Title = vm.Title;
        announcement.Content = vm.Content;
        announcement.IsActive = vm.IsActive;
        announcement.IsPinned = vm.IsPinned;
        announcement.StartDate = vm.StartDate;
        announcement.EndDate = vm.EndDate;

        // 刪除指定的附件
        if (vm.DeleteAttachmentIds is { Count: > 0 })
        {
            var toRemove = announcement.Attachments
                .Where(a => vm.DeleteAttachmentIds.Contains(a.Id)).ToList();
            foreach (var att in toRemove)
                announcement.Attachments.Remove(att);
        }

        // 新增附件
        if (newAttachments is { Count: > 0 })
        {
            var maxSort = announcement.Attachments.Any()
                ? announcement.Attachments.Max(a => a.SortOrder) + 1
                : 0;
            for (var i = 0; i < newAttachments.Count; i++)
            {
                var (fileUrl, fileName, fileSize) = newAttachments[i];
                announcement.Attachments.Add(new AnnouncementAttachment
                {
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileSizeBytes = fileSize,
                    SortOrder = maxSort + i
                });
            }
        }

        uow.Announcements.Update(announcement);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("公告更新成功 | AnnouncementId={AnnouncementId}", vm.Id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        logger.LogInformation("軟刪除公告 | AnnouncementId={AnnouncementId}", id);
        var announcement = await uow.Announcements.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (announcement is null)
            return ServiceResult.Failure("公告不存在");

        uow.Announcements.Remove(announcement); // 由 DbContext 攔截轉為軟刪除
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("公告軟刪除成功 | AnnouncementId={AnnouncementId}", id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> RestoreAsync(int id, CancellationToken ct = default)
    {
        logger.LogInformation("復原公告 | AnnouncementId={AnnouncementId}", id);
        var announcement = await uow.Announcements.GetWithAttachmentsIncludeDeletedAsync(id, ct).ConfigureAwait(false);
        if (announcement is null)
            return ServiceResult.Failure("公告不存在");

        announcement.IsDeleted = false;
        announcement.DeletedAt = null;
        uow.Announcements.Update(announcement);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("公告復原成功 | AnnouncementId={AnnouncementId}", id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> TogglePinAsync(int id, CancellationToken ct = default)
    {
        var announcement = await uow.Announcements.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (announcement is null)
            return ServiceResult.Failure("公告不存在");

        announcement.IsPinned = !announcement.IsPinned;
        uow.Announcements.Update(announcement);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("切換置頂 | AnnouncementId={AnnouncementId} | IsPinned={IsPinned}", id, announcement.IsPinned);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ReorderAsync(
        IReadOnlyList<int> orderedIds, CancellationToken ct = default)
    {
        logger.LogInformation("重新排序公告 | Count={Count}", orderedIds.Count);
        for (var i = 0; i < orderedIds.Count; i++)
        {
            var announcement = await uow.Announcements.GetByIdAsync(orderedIds[i], ct).ConfigureAwait(false);
            if (announcement is not null && announcement.SortOrder != i)
            {
                announcement.SortOrder = i;
                uow.Announcements.Update(announcement);
            }
        }

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    // ── 私有輔助 ──

    private static AnnouncementCardViewModel MapToCard(Announcement a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        ContentPreview = a.Content.Length > 100 ? a.Content[..100] + "..." : a.Content,
        IsPinned = a.IsPinned,
        HasAttachment = a.Attachments.Count > 0,
        CreatedAt = a.CreatedAt
    };

    private static AnnouncementAttachmentViewModel MapAttachment(AnnouncementAttachment a) => new()
    {
        Id = a.Id,
        FileName = a.FileName,
        FileUrl = a.FileUrl,
        FileSizeDisplay = FormatFileSize(a.FileSizeBytes)
    };

    private static string FormatFileSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024.0):F1} MB"
    };
}
