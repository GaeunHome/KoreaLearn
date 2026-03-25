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
            logger.LogWarning("找不到資源: {Message}", ex.Message);
            context.Response.StatusCode = 404;
            context.Response.Redirect("/Error/NotFound");
        }
        catch (BusinessException ex)
        {
            logger.LogWarning("業務規則違反: {Message}", ex.Message);
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
            logger.LogError(ex, "未預期的系統錯誤 Path={Path}", context.Request.Path);
            context.Response.StatusCode = 500;
            context.Response.Redirect("/Error");
        }
    }
}
