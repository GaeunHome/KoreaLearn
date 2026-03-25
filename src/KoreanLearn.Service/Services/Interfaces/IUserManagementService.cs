using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.User;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IUserManagementService
{
    Task<PagedResult<UserListViewModel>> GetUsersPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<UserDetailViewModel?> GetUserDetailAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult> PromoteToTeacherAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult> DemoteFromTeacherAsync(string userId, CancellationToken ct = default);
}
