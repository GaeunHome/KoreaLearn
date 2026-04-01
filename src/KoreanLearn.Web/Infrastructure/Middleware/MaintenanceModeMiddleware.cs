namespace KoreanLearn.Web.Infrastructure.Middleware;

/// <summary>
/// 維護模式中介軟體，從 AppSettings:MaintenanceMode 讀取開關。
/// Admin 角色放行，靜態資源與 /health 不攔截。
/// </summary>
public class MaintenanceModeMiddleware(RequestDelegate next, IConfiguration config, ILogger<MaintenanceModeMiddleware> logger)
{
    private static readonly string[] ExcludedPrefixes =
        ["/Admin", "/Identity/Account/Login", "/health", "/lib/", "/css/", "/js/", "/images/", "/uploads/"];

    public async Task InvokeAsync(HttpContext context)
    {
        var isMaintenanceMode = config.GetValue<bool>("AppSettings:MaintenanceMode");
        if (!isMaintenanceMode)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "";

        // 排除路徑
        if (ExcludedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        // Admin 角色放行
        if (context.User.IsInRole("Admin"))
        {
            await next(context);
            return;
        }

        // 回傳 503 維護頁面
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        logger.LogInformation("維護模式攔截請求 | Path={Path} | IP={IP}", path, ip);
        var message = config["AppSettings:MaintenanceMessage"] ?? "系統維護中，請稍後再試。";
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(GenerateMaintenanceHtml(message));
    }

    private static string GenerateMaintenanceHtml(string message) =>
        $"""
        <!DOCTYPE html>
        <html lang="zh-Hant">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>系統維護中 - KoreanLearn</title>
            <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
            <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
        </head>
        <body class="bg-light d-flex align-items-center justify-content-center" style="min-height:100vh;">
            <div class="text-center">
                <i class="bi bi-tools" style="font-size:4rem;color:#2B3A67;"></i>
                <h2 class="mt-3" style="color:#2B3A67;">系統維護中</h2>
                <p class="text-muted">{System.Net.WebUtility.HtmlEncode(message)}</p>
            </div>
        </body>
        </html>
        """;
}

/// <summary>MaintenanceModeMiddleware 擴充方法</summary>
public static class MaintenanceModeMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder app)
        => app.UseMiddleware<MaintenanceModeMiddleware>();
}
