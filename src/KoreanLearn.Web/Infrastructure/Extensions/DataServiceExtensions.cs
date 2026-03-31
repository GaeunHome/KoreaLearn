using KoreanLearn.Data;
using KoreanLearn.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace KoreanLearn.Web.Infrastructure.Extensions;

/// <summary>資料層服務註冊擴充方法，註冊 DbContext、Repository、UoW 與 ASP.NET Core Identity</summary>
public static class DataServiceExtensions
{
    /// <summary>註冊資料層服務（DbContext、Repository、UoW）與 Identity 認證系統</summary>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DefaultConnection");

        // Data 層自行註冊 DbContext、Repositories、UoW
        services.AddDataLayer(connectionString);

        // Identity 需要 Microsoft.AspNetCore.Identity.UI，放 Web 層註冊
        services.AddDefaultIdentity<AppUser>(opts =>
            {
                // 密碼強度（CWE-521）
                opts.Password.RequireDigit = true;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequiredLength = 6;

                // 使用者設定
                opts.User.RequireUniqueEmail = true;

                // Email 驗證（註冊後需先驗證 Email 才能登入）
                opts.SignIn.RequireConfirmedEmail = true;

                // 帳號鎖定（CWE-307：暴力破解防護）
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opts.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
