namespace KoreanLearn.Web.Infrastructure.Middleware;

/// <summary>
/// 安全標頭中介軟體
/// 為所有 HTTP 回應加入安全相關的標頭，防止常見的 Web 攻擊
/// 對應 OWASP Top 10: A05（安全設定錯誤）
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>在回應中注入安全標頭後繼續管線處理</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // ─── X-Content-Type-Options ─────────────────────────────
        // 防止瀏覽器 MIME 嗅探，避免將非腳本檔案（如圖片）誤判為可執行腳本
        // 對應 CWE-16: Configuration
        headers.Append("X-Content-Type-Options", "nosniff");

        // ─── X-Frame-Options ─────────────────────────────────────
        // 防止點擊劫持（Clickjacking）：禁止網站被嵌入到其他網站的 <iframe> 中
        // DENY = 完全禁止；搭配 CSP frame-ancestors 'none' 形成雙重防護
        // 對應 CWE-1021: Improper Restriction of Rendered UI Layers
        headers.Append("X-Frame-Options", "DENY");

        // ─── Referrer-Policy ─────────────────────────────────────
        // 控制 HTTP Referer 標頭的傳送策略，防止敏感 URL 洩漏給第三方
        // strict-origin-when-cross-origin：同源請求傳完整 URL，跨域僅傳 origin
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // ─── X-Permitted-Cross-Domain-Policies ───────────────────
        // 禁止 Flash/PDF 等外掛載入跨域策略檔案
        headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        // ─── Permissions-Policy ──────────────────────────────────
        // 限制瀏覽器功能（攝影機、麥克風、地理位置、付款）的使用權限
        // 語言學習平台不需要這些功能，全部禁用以縮小攻擊面
        headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");

        // ─── Content-Security-Policy ─────────────────────────────
        // 內容安全政策：限制頁面可載入的資源來源，防止 XSS 攻擊
        // 對應 CWE-79: Improper Neutralization of Input During Web Page Generation
        //
        // - default-src 'self'：預設只允許同源資源
        // - script-src 'unsafe-inline'：因部分頁面使用行內腳本（後續可改為 nonce）
        // - style-src 'unsafe-inline'：因 Bootstrap / Quill.js 使用行內樣式
        // - img-src data:：因部分圖片使用 Base64 編碼
        // - form-action：限制表單僅能提交至白名單域名，對抗 CSRF 的第三道防線
        //   （第一道：SameSite Cookie；第二道：AntiForgeryToken；第三道：此 CSP directive）
        // - frame-ancestors 'none'：防止 Clickjacking，與 X-Frame-Options: DENY 互補
        // - base-uri 'self'：防止攻擊者注入 <base> 標籤劫持相對路徑
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
