using KoreanLearn.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KoreanLearn.Data;

/// <summary>資料層 DI 擴充方法，註冊 DbContext、DbContextFactory 與 UnitOfWork</summary>
public static class DependencyInjection
{
    /// <summary>將資料層所有服務註冊至 DI 容器</summary>
    public static IServiceCollection AddDataLayer(
        this IServiceCollection services, string connectionString)
    {
        // ── DbContext（Identity 使用）─────────────────────
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(connectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        // ── DbContextFactory（UnitOfWork + 平行查詢 + BackgroundService 使用）
        services.AddDbContextFactory<ApplicationDbContext>(opts =>
            opts.UseSqlServer(connectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)),
            lifetime: ServiceLifetime.Scoped);

        // ── UnitOfWork（透過 Factory 建立 DbContext，Repository 延遲初始化）
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}
