using System.IO.Compression;
using System.Threading.RateLimiting;
using KoreanLearn.Data;
using KoreanLearn.Service.Services.Implementation;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Web.Infrastructure.Extensions;
using KoreanLearn.Web.Infrastructure.Middleware;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Serilog.Events;

// ── Serilog 結構化日誌設定 ──
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("KoreanLearn 應用程式啟動中...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ── 服務註冊 ──
    builder.Services
        .AddDataServices(builder.Configuration)   // 資料層：DbContext、Repository、UoW、Identity
        .AddApplicationServices();                 // 應用層：AutoMapper、所有 Service

    // ── SMTP Email 服務 ──
    builder.Services.Configure<KoreanLearn.Web.Infrastructure.Settings.SmtpSettings>(
        builder.Configuration.GetSection("SmtpSettings"));
    builder.Services.AddSingleton<KoreanLearn.Service.Services.Interfaces.IEmailService,
        KoreanLearn.Web.Infrastructure.Services.SmtpEmailService>();

    // ── 檔案上傳服務 ──
    builder.Services.AddScoped<KoreanLearn.Service.Services.Interfaces.IFileUploadService,
        KoreanLearn.Web.Infrastructure.Services.FileUploadService>();

    // 背景服務：每日維護任務（字卡複習統計、過期訂閱停用）
    builder.Services.AddHostedService<KoreanLearn.Web.Infrastructure.BackgroundServices.DailyMaintenanceService>();

    // ── 速率限制 ──
    builder.Services.AddRateLimiter(opts =>
    {
        opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // 認證端點：每 IP 每分鐘最多 10 次請求
        opts.AddPolicy("auth", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        // API 端點：每 IP 每分鐘最多 30 次請求
        opts.AddPolicy("api", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 4
                }));

        // 全域限流：每 IP 每分鐘最多 100 次請求（滑動視窗）
        opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 4
                }));
    });

    // ── Cookie 安全設定 ──
    // 涵蓋 OWASP Top 10 中 A01（存取控制）、A03（XSS）、A07（認證）相關項目
    builder.Services.ConfigureApplicationCookie(opts =>
    {
        opts.LoginPath = "/Identity/Account/Login";
        opts.LogoutPath = "/Identity/Account/Logout";
        opts.AccessDeniedPath = "/Error/403";

        // ─── Session 過期控制（CWE-613: Insufficient Session Expiration）───
        // 閒置超過 2 小時自動失效；SlidingExpiration 讓持續操作的使用者不被強制登出
        opts.ExpireTimeSpan = TimeSpan.FromHours(2);
        opts.SlidingExpiration = true;

        // ─── HttpOnly（CWE-1004: Sensitive Cookie Without HttpOnly Flag）───
        // 禁止 JavaScript 透過 document.cookie 讀取認證 Cookie，
        // 即使存在 XSS 漏洞，攻擊者腳本也無法竊取 Cookie 送往外部伺服器
        opts.Cookie.HttpOnly = true;

        // ─── Secure（CWE-614: Sensitive Cookie Without Secure Attribute）──
        // Cookie 僅在 HTTPS 加密連線時傳送，防止 MITM 攔截明文 Cookie
        // SameAsRequest：開發環境 http 可用，正式環境自動套用 Secure flag
        opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        // ─── SameSite（CWE-1275: Improper SameSite Attribute）─────────────
        // Lax：阻擋跨站 POST 表單（CSRF 主要攻擊向量），允許 GET 導覽帶 Cookie
        // 搭配 [ValidateAntiForgeryToken] + AddAntiforgery 形成雙重 CSRF 防護
        opts.Cookie.SameSite = SameSiteMode.Lax;

        // ─── 自訂名稱（CWE-200: Information Disclosure）────────────────────
        // 隱藏 .AspNetCore.Identity.Application 預設名稱，降低被自動化工具識別框架的風險
        opts.Cookie.Name = "KoreanLearn.Auth";
    });

    // ── 快取服務 ──
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, CacheService>();

    // ── 回應壓縮（Brotli + Gzip）──
    builder.Services.AddResponseCompression(opts =>
    {
        opts.EnableForHttps = true;
        opts.Providers.Add<BrotliCompressionProvider>();
        opts.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(opts => opts.Level = CompressionLevel.Fastest);
    builder.Services.Configure<GzipCompressionProviderOptions>(opts => opts.Level = CompressionLevel.Fastest);

    builder.Services.AddResponseCaching();
    // ── Session 設定（CWE-614 / CWE-1004）──
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(20);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "KoreanLearn.Session";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

    // ── CSRF 防護設定（CWE-352: Cross-Site Request Forgery）──
    // 搭配 [ValidateAntiForgeryToken] 形成雙重防護：
    // Cookie 驗證（SameSite=Strict）+ 請求 Token 比對
    // AJAX 呼叫透過 X-CSRF-TOKEN 標頭傳遞 Token
    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.Name = "KoreanLearn.Csrf";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.HeaderName = "X-CSRF-TOKEN";   // AJAX fetch/XHR 使用此標頭
    });

    // ── HSTS 設定（CWE-319: Cleartext Transmission）──
    // 告知瀏覽器未來 365 天內只能透過 HTTPS 連線（含子網域）
    // 防止 SSL Stripping 攻擊；僅正式環境由 UseHsts() 啟用
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });

    builder.Services.AddControllersWithViews();

    // ── 健康檢查 ──
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

    // ── 社群登入 ──
    var authSection = builder.Configuration.GetSection("Authentication");
    var authBuilder = builder.Services.AddAuthentication();

    // ── Google OAuth 社群登入 ──
    if (!string.IsNullOrEmpty(authSection["Google:ClientId"]))
    {
        authBuilder.AddGoogle(options =>
        {
            options.ClientId = authSection["Google:ClientId"]!;
            options.ClientSecret = authSection["Google:ClientSecret"]!;
        });
    }

    // ── Facebook OAuth 社群登入 ──
    if (!string.IsNullOrEmpty(authSection["Facebook:AppId"]))
    {
        authBuilder.AddFacebook(options =>
        {
            options.AppId = authSection["Facebook:AppId"]!;
            options.AppSecret = authSection["Facebook:AppSecret"]!;
        });
    }

    // ── Apple ID 社群登入（自訂 OAuth）──
    if (!string.IsNullOrEmpty(authSection["Apple:ClientId"]))
    {
        authBuilder.AddOAuth("Apple", "Apple", options =>
        {
            options.ClientId = authSection["Apple:ClientId"]!;
            options.ClientSecret = authSection["Apple:PrivateKey"]!; // Apple 使用 JWT Client Secret
            options.AuthorizationEndpoint = "https://appleid.apple.com/auth/authorize";
            options.TokenEndpoint = "https://appleid.apple.com/auth/token";
            options.CallbackPath = "/signin-apple";
            options.Scope.Add("name");
            options.Scope.Add("email");
            options.Events.OnCreatingTicket = context =>
            {
                var idToken = context.TokenResponse.Response?.RootElement.GetProperty("id_token").GetString();
                if (!string.IsNullOrEmpty(idToken))
                {
                    try
                    {
                        var parts = idToken.Split('.');
                        if (parts.Length >= 2)
                        {
                            var payload = parts[1];
                            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                            var json = System.Text.Encoding.UTF8.GetString(
                                Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/')));
                            var doc = System.Text.Json.JsonDocument.Parse(json);

                            if (doc.RootElement.TryGetProperty("sub", out var subProp))
                                context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                    System.Security.Claims.ClaimTypes.NameIdentifier, subProp.GetString()!));
                            if (doc.RootElement.TryGetProperty("email", out var emailProp))
                                context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                    System.Security.Claims.ClaimTypes.Email, emailProp.GetString()!));
                        }
                    }
                    catch { /* id_token 解析失敗 */ }
                }
                return Task.CompletedTask;
            };
        });
    }

    // ── LINE Login 社群登入 ──
    if (!string.IsNullOrEmpty(authSection["LINE:ChannelId"]))
    {
        authBuilder.AddOAuth("LINE", "LINE", options =>
        {
            options.ClientId = authSection["LINE:ChannelId"]!;
            options.ClientSecret = authSection["LINE:ChannelSecret"]!;
            options.AuthorizationEndpoint = "https://access.line.me/oauth2/v2.1/authorize";
            options.TokenEndpoint = "https://api.line.me/oauth2/v2.1/token";
            options.UserInformationEndpoint = "https://api.line.me/v2/profile";
            options.CallbackPath = "/signin-line";
            options.Scope.Add("profile");
            options.Scope.Add("openid");
            options.Scope.Add("email");

            options.Events.OnCreatingTicket = async context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                // 取得 LINE 使用者資訊
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                    var response = await context.Backchannel.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                    // 從 LINE profile API 回應手動映射 claims
                    if (user.RootElement.TryGetProperty("userId", out var userIdProp))
                        context.Identity?.AddClaim(new System.Security.Claims.Claim(
                            System.Security.Claims.ClaimTypes.NameIdentifier, userIdProp.GetString()!));
                    if (user.RootElement.TryGetProperty("displayName", out var nameProp))
                        context.Identity?.AddClaim(new System.Security.Claims.Claim(
                            System.Security.Claims.ClaimTypes.Name, nameProp.GetString()!));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "LINE Profile API 呼叫失敗");
                    throw;
                }

                // 從 id_token 中解析 email（LINE 的 email 不在 profile API 中）
                if (context.TokenResponse.Response?.RootElement.TryGetProperty("id_token", out var idTokenElement) == true)
                {
                    var idToken = idTokenElement.GetString();
                    if (!string.IsNullOrEmpty(idToken))
                    {
                        try
                        {
                            var parts = idToken.Split('.');
                            if (parts.Length >= 2)
                            {
                                var payload = parts[1];
                                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                                var json = System.Text.Encoding.UTF8.GetString(
                                    Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/')));
                                var doc = System.Text.Json.JsonDocument.Parse(json);
                                if (doc.RootElement.TryGetProperty("email", out var emailProp))
                                {
                                    var email = emailProp.GetString();
                                    if (!string.IsNullOrEmpty(email))
                                        context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                            System.Security.Claims.ClaimTypes.Email, email));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "LINE id_token 解析失敗，帳號將無 Email");
                        }
                    }
                }
            };
        });
    }

    var app = builder.Build();

    // ── HTTP 請求管線設定 ──
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // 全域例外處理（在所有其他中介軟體之前）
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseStatusCodePagesWithReExecute("/Error/{0}");  // HTTP 狀態碼錯誤頁面
    app.UseCorrelationId();                              // 關聯識別碼（請求追蹤）
    app.UseSecurityHeaders();                            // 安全標頭（CSP、X-Frame-Options 等）
    app.UseResponseCompression();                        // 回應壓縮（Brotli + Gzip）
    app.UseResponseCaching();                            // 回應快取
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();                                    // Session（在 Routing 之後、Auth 之前）
    app.UseRateLimiter();                                // 速率限制
    app.UseAuthentication();                             // 身份驗證
    app.UseAuthorization();                              // 授權

    // Serilog 請求日誌（在 MaintenanceMode 之前，確保維護模式請求也被記錄）
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000}ms";
    });

    app.UseMaintenanceMode();                            // 維護模式

    // ── 路由設定 ──
    app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    app.MapHealthChecks("/health");

    // ── 種子資料初始化 ──
    await DbInitializer.InitializeAsync(app.Services);

    Log.Information("KoreanLearn 應用程式已啟動");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "KoreanLearn 應用程式啟動失敗");
}
finally
{
    Log.CloseAndFlush();
}
