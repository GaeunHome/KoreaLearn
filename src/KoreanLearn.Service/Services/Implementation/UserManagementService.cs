using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>後台使用者管理業務邏輯實作，處理使用者查詢、教師角色升降級</summary>
public class UserManagementService(
    UserManager<AppUser> userManager,
    IUnitOfWork uow,
    ILogger<UserManagementService> logger) : IUserManagementService
{
    /// <inheritdoc />
    public async Task<PagedResult<UserListViewModel>> GetUsersPagedAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email!.Contains(search) || u.DisplayName.Contains(search));

        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var users = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct).ConfigureAwait(false);

        var items = new List<UserListViewModel>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u).ConfigureAwait(false);
            items.Add(new UserListViewModel
            {
                Id = u.Id, Email = u.Email ?? "", DisplayName = u.DisplayName,
                Roles = roles.ToList(), CreatedAt = u.CreatedAt
            });
        }

        return new PagedResult<UserListViewModel>(items, total, page, pageSize);
    }

    /// <inheritdoc />
    public async Task<UserDetailViewModel?> GetUserDetailAsync(
        string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var enrollments = await uow.Enrollments.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
        var orders = await uow.Orders.GetByUserIdAsync(userId, ct).ConfigureAwait(false);

        return new UserDetailViewModel
        {
            Id = user.Id, Email = user.Email ?? "", DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList(), CreatedAt = user.CreatedAt,
            EnrollmentCount = enrollments.Count,
            OrderCount = orders.Count
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult> PromoteToTeacherAsync(
        string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        if (await userManager.IsInRoleAsync(user, "Teacher").ConfigureAwait(false))
            return ServiceResult.Failure("該使用者已是教師");

        var result = await userManager.AddToRoleAsync(user, "Teacher").ConfigureAwait(false);
        if (!result.Succeeded)
            return ServiceResult.Failure("升級失敗");

        logger.LogInformation("使用者升級為教師 | UserId={UserId} | Email={Email}", userId, user.Email);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DemoteFromTeacherAsync(
        string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return ServiceResult.Failure("使用者不存在");

        if (!await userManager.IsInRoleAsync(user, "Teacher").ConfigureAwait(false))
            return ServiceResult.Failure("該使用者不是教師");

        var result = await userManager.RemoveFromRoleAsync(user, "Teacher").ConfigureAwait(false);
        if (!result.Succeeded)
            return ServiceResult.Failure("降級失敗");

        logger.LogInformation("使用者從教師降級 | UserId={UserId} | Email={Email}", userId, user.Email);
        return ServiceResult.Success();
    }
}
