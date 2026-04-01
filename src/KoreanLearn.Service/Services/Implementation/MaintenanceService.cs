using KoreanLearn.Data;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>系統維護業務邏輯實作，供背景排程服務使用（使用 IDbContextFactory 確保 thread-safe）</summary>
public class MaintenanceService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<MaintenanceService> logger) : IMaintenanceService
{
    /// <inheritdoc />
    public async Task<int> DeactivateExpiredSubscriptionsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("開始檢查過期訂閱");
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        // 查詢所有已到期但仍為啟用狀態的訂閱
        var expiredSubs = await db.UserSubscriptions
            .Where(s => s.IsActive && s.EndDate <= DateTime.UtcNow)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        foreach (var sub in expiredSubs)
        {
            sub.IsActive = false;
            logger.LogInformation("訂閱到期 | UserId={UserId} | PlanId={PlanId}", sub.UserId, sub.PlanId);
        }

        if (expiredSubs.Count > 0)
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("過期訂閱檢查完成 | DeactivatedCount={Count}", expiredSubs.Count);
        return expiredSubs.Count;
    }

    /// <inheritdoc />
    public async Task<int> CountDueFlashcardReviewsAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        // 統計所有已到複習時間的字卡學習紀錄
        var count = await db.FlashcardLogs
            .Where(l => l.NextReviewDate <= DateTime.UtcNow)
            .CountAsync(ct)
            .ConfigureAwait(false);

        logger.LogInformation("到期字卡複習統計 | DueCount={DueCount}", count);
        return count;
    }
}
