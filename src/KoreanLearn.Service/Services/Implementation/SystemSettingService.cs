using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.SystemSetting;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>系統參數管理服務實作</summary>
public class SystemSettingService(IUnitOfWork uow, ILogger<SystemSettingService> logger) : ISystemSettingService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SystemSettingViewModel>> GetAllSettingsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("取得所有系統參數");
        var settings = await uow.SystemSettings.GetAllOrderedAsync(ct).ConfigureAwait(false);
        return settings.Select(s => new SystemSettingViewModel
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description,
            Group = s.Group,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<SystemSettingFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        logger.LogInformation("取得系統參數編輯資料 | SettingId={SettingId}", id);
        var s = await uow.SystemSettings.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (s is null) return null;
        return new SystemSettingFormViewModel
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description,
            Group = s.Group
        };
    }

    /// <inheritdoc />
    public async Task<string?> GetValueAsync(string key, CancellationToken ct = default)
    {
        var s = await uow.SystemSettings.GetByKeyAsync(key, ct).ConfigureAwait(false);
        return s?.Value;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateAsync(SystemSettingFormViewModel vm, CancellationToken ct = default)
    {
        var existing = await uow.SystemSettings.GetByKeyAsync(vm.Key, ct).ConfigureAwait(false);
        if (existing is not null)
            return ServiceResult<int>.Failure($"參數鍵 '{vm.Key}' 已存在");

        var entity = new SystemSetting
        {
            Key = vm.Key,
            Value = vm.Value,
            Description = vm.Description,
            Group = vm.Group,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.SystemSettings.AddAsync(entity, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("系統參數建立 | Key={Key} | Value={Value} | Group={Group}", vm.Key, vm.Value, vm.Group);
        return ServiceResult<int>.Success(entity.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateAsync(SystemSettingFormViewModel vm, CancellationToken ct = default)
    {
        var entity = await uow.SystemSettings.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (entity is null)
        {
            logger.LogWarning("更新系統參數失敗：不存在 | Id={Id}", vm.Id);
            return ServiceResult.Failure("參數不存在");
        }

        entity.Value = vm.Value;
        entity.Description = vm.Description;
        entity.Group = vm.Group;
        entity.UpdatedAt = DateTime.UtcNow;

        uow.SystemSettings.Update(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("系統參數更新 | Key={Key} | Value={Value}", entity.Key, vm.Value);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.SystemSettings.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (entity is null)
        {
            logger.LogWarning("刪除系統參數失敗：不存在 | Id={Id}", id);
            return ServiceResult.Failure("參數不存在");
        }

        uow.SystemSettings.Remove(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("系統參數刪除 | Key={Key}", entity.Key);
        return ServiceResult.Success();
    }
}
