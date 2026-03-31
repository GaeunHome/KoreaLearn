namespace KoreanLearn.Web.Infrastructure.Middleware;

/// <summary>安全標頭中介軟體，為所有回應加入安全性 HTTP 標頭（CSP、X-Frame-Options 等）</summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>在回應中注入安全標頭後繼續管線處理</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.Append("X-Content-Type-Options", "nosniff");
        headers.Append("X-Frame-Options", "DENY");
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");

        headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.gstatic.com; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self' https://accounts.google.com https://www.facebook.com https://access.line.me https://appleid.apple.com");

        await next(context);
    }
}

/// <summary>SecurityHeadersMiddleware 的擴充方法，提供簡潔的管線註冊語法</summary>
public static class SecurityHeadersExtensions
{
    /// <summary>將安全標頭中介軟體加入 HTTP 請求管線</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
