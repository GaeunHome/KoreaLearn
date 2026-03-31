using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.SystemSetting;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>系統參數管理服務介面</summary>
public interface ISystemSettingService
{
    /// <summary>取得所有系統參數</summary>
    Task<IReadOnlyList<SystemSettingViewModel>> GetAllSettingsAsync(CancellationToken ct = default);

    /// <summary>取得單一參數（用於編輯表單）</summary>
    Task<SystemSettingFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default);

    /// <summary>依鍵取得參數值</summary>
    Task<string?> GetValueAsync(string key, CancellationToken ct = default);

    /// <summary>建立新參數</summary>
    Task<ServiceResult<int>> CreateAsync(SystemSettingFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新參數</summary>
    Task<ServiceResult> UpdateAsync(SystemSettingFormViewModel vm, CancellationToken ct = default);

    /// <summary>刪除參數</summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
