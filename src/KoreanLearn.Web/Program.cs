using System.Threading.RateLimiting;
using KoreanLearn.Data;
using KoreanLearn.Web.Infrastructure.Extensions;
using KoreanLearn.Web.Infrastructure.Middleware;
using Serilog;
using Serilog.Events;

// ── Serilog 結構化日誌設定 ──
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
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
    builder.Services.ConfigureApplicationCookie(opts =>
    {
        opts.Cookie.HttpOnly = true;
        opts.Cookie.SameSite = SameSiteMode.Lax;
        opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opts.Cookie.Name = "KoreanLearn.Auth";
        opts.ExpireTimeSpan = TimeSpan.FromHours(2);
        opts.SlidingExpiration = true;
        opts.LoginPath = "/Identity/Account/Login";
        opts.LogoutPath = "/Identity/Account/Logout";
        opts.AccessDeniedPath = "/Error/403";
    });

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    var app = builder.Build();

    // ── HTTP 請求管線設定 ──
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/Error/{0}");  // HTTP 狀態碼錯誤頁面
    app.UseSecurityHeaders();                            // 安全標頭（CSP、X-Frame-Options 等）
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();                                // 速率限制
    app.UseAuthentication();                             // 身份驗證
    app.UseAuthorization();                              // 授權

    // Serilog 請求日誌
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000}ms";
    });

    // 全域例外處理中介軟體
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // ── 路由設定 ──
    app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    app.MapRazorPages();

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
