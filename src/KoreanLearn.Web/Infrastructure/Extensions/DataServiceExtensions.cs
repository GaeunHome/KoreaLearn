using KoreanLearn.Data;
using KoreanLearn.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace KoreanLearn.Web.Infrastructure.Extensions;

public static class DataServiceExtensions
{
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
                opts.Password.RequireDigit = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequiredLength = 6;
                opts.User.RequireUniqueEmail = true;
                opts.SignIn.RequireConfirmedEmail = false;
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }
}
