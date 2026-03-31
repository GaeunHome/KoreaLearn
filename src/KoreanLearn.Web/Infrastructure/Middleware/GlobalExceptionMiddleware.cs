using KoreanLearn.Library.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Web.Infrastructure.Middleware;

/// <summary>全域例外處理中介軟體，攔截未處理的例外並依類型導向對應錯誤頁面</summary>
/// <remarks>
/// 處理邏輯：
/// - NotFoundException → 404 導向 /Error/NotFound
/// - BusinessException → 422（JSON）或導向 /Error
/// - 其他 Exception → 500 導向 /Error
/// </remarks>
public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    /// <summary>執行中介軟體管線，攔截例外並記錄結構化日誌</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning("找不到資源 | Path={Path} | Message={Message} | User={User}",
                context.Request.Path, ex.Message, context.User.Identity?.Name ?? "Anonymous");
            context.Response.StatusCode = 404;
            context.Response.Redirect("/Error/NotFound");
        }
        catch (BusinessException ex)
        {
            logger.LogWarning("業務規則違反 | Path={Path} | Message={Message} | User={User}",
                context.Request.Path, ex.Message, context.User.Identity?.Name ?? "Anonymous");

            if (context.Request.Headers.Accept.Contains("application/json"))
            {
                context.Response.StatusCode = 422;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                return;
            }
            context.Response.Redirect("/Error");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "並行衝突 | Path={Path}", context.Request.Path);
            context.Response.Redirect("/Error/409");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "未預期的系統錯誤 | Path={Path} | Method={Method} | User={User} | QueryString={Query}",
                context.Request.Path,
                context.Request.Method,
                context.User.Identity?.Name ?? "Anonymous",
                context.Request.QueryString);
            context.Response.StatusCode = 500;
            context.Response.Redirect("/Error");
        }
    }
}
