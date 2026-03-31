using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KoreanLearn.Web.Controllers.Api;

/// <summary>API 控制器基底，提供速率限制與使用者 ID 輔助方法</summary>
[ApiController]
[EnableRateLimiting("api")]
public abstract class BaseApiController : ControllerBase
{
    protected string? GetCurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected string GetAuthorizedUserId()
        => GetCurrentUserId() ?? throw new UnauthorizedAccessException("使用者未登入");
}
