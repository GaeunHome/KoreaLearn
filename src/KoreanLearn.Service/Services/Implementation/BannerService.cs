using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Banner;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>首頁幻燈片管理服務實作</summary>
public class BannerService(IUnitOfWork uow, ILogger<BannerService> logger) : IBannerService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<BannerViewModel>> GetActiveBannersAsync(CancellationToken ct = default)
    {
        var banners = await uow.Banners.GetActiveOrderedAsync(ct).ConfigureAwait(false);
        return banners.Select(MapToVm).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BannerViewModel>> GetAllBannersAsync(CancellationToken ct = default)
    {
        var banners = await uow.Banners.GetAllAsync(ct).ConfigureAwait(false);
        return banners.Select(MapToVm).ToList();
    }

    /// <inheritdoc />
    public async Task<BannerFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var b = await uow.Banners.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (b is null) return null;
        return new BannerFormViewModel
        {
            Id = b.Id,
            Title = b.Title,
            ImageUrl = b.ImageUrl,
            CourseId = b.CourseId,
            DisplayOrder = b.DisplayOrder,
            IsActive = b.IsActive
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateAsync(BannerFormViewModel vm, CancellationToken ct = default)
    {
        var entity = new Banner
        {
            Title = vm.Title,
            ImageUrl = vm.ImageUrl,
            CourseId = vm.CourseId,
            DisplayOrder = vm.DisplayOrder,
            IsActive = vm.IsActive
        };

        await uow.Banners.AddAsync(entity, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("Banner 建立 | Id={Id}", entity.Id);
        return ServiceResult<int>.Success(entity.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateAsync(BannerFormViewModel vm, CancellationToken ct = default)
    {
        var entity = await uow.Banners.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (entity is null) return ServiceResult.Failure("幻燈片不存在");

        entity.Title = vm.Title;
        entity.ImageUrl = vm.ImageUrl;
        entity.CourseId = vm.CourseId;
        entity.DisplayOrder = vm.DisplayOrder;
        entity.IsActive = vm.IsActive;

        uow.Banners.Update(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("Banner 更新 | Id={Id}", vm.Id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Banners.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (entity is null) return ServiceResult.Failure("幻燈片不存在");

        uow.Banners.Remove(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("Banner 刪除 | Id={Id}", id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Banners.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (entity is null) return ServiceResult.Failure("幻燈片不存在");

        entity.IsActive = !entity.IsActive;
        uow.Banners.Update(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("Banner 狀態切換 | Id={Id}, IsActive={IsActive}", id, entity.IsActive);
        return ServiceResult.Success();
    }

    private static BannerViewModel MapToVm(Banner b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        ImageUrl = b.ImageUrl,
        CourseId = b.CourseId,
        CourseTitle = b.Course?.Title,
        DisplayOrder = b.DisplayOrder,
        IsActive = b.IsActive
    };
}
