using Serilog.Context;

namespace KoreanLearn.Web.Infrastructure.Middleware;

/// <summary>
/// 關聯識別碼中介軟體，為每個 HTTP 請求產生唯一 Correlation ID，
/// 貫穿整個請求生命週期的日誌，方便追蹤同一請求的所有操作。
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N")[..12];

        context.Response.Headers[CorrelationIdHeader] = correlationId;
        context.Items["CorrelationId"] = correlationId;

        logger.LogDebug("CorrelationId 已指派 | CorrelationId={CorrelationId}", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}

/// <summary>CorrelationIdMiddleware 擴充方法</summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
