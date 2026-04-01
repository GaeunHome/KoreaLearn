using System.Security.Claims;
using KoreanLearn.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace KoreanLearn.Web.Infrastructure;

/// <summary>
/// 自訂 Claims Principal Factory，在使用者登入時將 DisplayName 寫入 claims，
/// 供 Partial View 直接讀取，避免每次頁面渲染都查詢資料庫。
/// </summary>
public class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<AppUser, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user).ConfigureAwait(false);
        identity.AddClaim(new Claim("DisplayName", user.DisplayName));
        return identity;
    }
}
