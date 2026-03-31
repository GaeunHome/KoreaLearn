using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Banner;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>首頁幻燈片管理服務介面</summary>
public interface IBannerService
{
    /// <summary>取得所有已啟用的幻燈片（前台用）</summary>
    Task<IReadOnlyList<BannerViewModel>> GetActiveBannersAsync(CancellationToken ct = default);

    /// <summary>取得所有幻燈片（後台管理用）</summary>
    Task<IReadOnlyList<BannerViewModel>> GetAllBannersAsync(CancellationToken ct = default);

    /// <summary>取得單一幻燈片（用於編輯表單）</summary>
    Task<BannerFormViewModel?> GetForEditAsync(int id, CancellationToken ct = default);

    /// <summary>建立新幻燈片</summary>
    Task<ServiceResult<int>> CreateAsync(BannerFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新幻燈片</summary>
    Task<ServiceResult> UpdateAsync(BannerFormViewModel vm, CancellationToken ct = default);

    /// <summary>刪除幻燈片（軟刪除）</summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>切換幻燈片啟用狀態</summary>
    Task<ServiceResult> ToggleActiveAsync(int id, CancellationToken ct = default);
}
