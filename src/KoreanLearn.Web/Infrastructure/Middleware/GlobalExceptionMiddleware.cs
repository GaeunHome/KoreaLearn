using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Web.Infrastructure.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
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
