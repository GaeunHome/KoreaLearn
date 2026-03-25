using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.User;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>後台使用者管理業務邏輯介面（使用者查詢、角色升降級）</summary>
public interface IUserManagementService
{
    /// <summary>取得使用者分頁列表（可依關鍵字搜尋）</summary>
    Task<PagedResult<UserListViewModel>> GetUsersPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得使用者詳情（含角色、選課數、訂單數）</summary>
    Task<UserDetailViewModel?> GetUserDetailAsync(string userId, CancellationToken ct = default);

    /// <summary>將使用者升級為教師角色</summary>
    Task<ServiceResult> PromoteToTeacherAsync(string userId, CancellationToken ct = default);

    /// <summary>將使用者從教師角色降級</summary>
    Task<ServiceResult> DemoteFromTeacherAsync(string userId, CancellationToken ct = default);
}
