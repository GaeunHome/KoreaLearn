using KoreanLearn.Data;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Web.Infrastructure.BackgroundServices;

public class DailyMaintenanceService(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyMaintenanceService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DailyMaintenanceService 啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "DailyMaintenanceService 執行錯誤");
            }

            // Wait until next day 2:00 AM UTC
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            var delay = nextRun - now;
            logger.LogInformation("下次執行：{NextRun}（{Delay} 後）", nextRun, delay);

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Check expired subscriptions
        var expiredSubs = await db.Set<KoreanLearn.Data.Entities.UserSubscription>()
            .Where(s => s.IsActive && s.EndDate <= DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var sub in expiredSubs)
        {
            sub.IsActive = false;
            logger.LogInformation("訂閱到期 | UserId={UserId} | PlanId={PlanId}", sub.UserId, sub.PlanId);
        }

        // 2. Log flashcard review stats
        var dueCards = await db.Set<KoreanLearn.Data.Entities.FlashcardLog>()
            .Where(l => l.NextReviewDate <= DateTime.UtcNow)
            .CountAsync(ct);
        logger.LogInformation("今日待複習字卡總數：{Count}", dueCards);

        if (expiredSubs.Count > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation("DailyMaintenanceService 完成 | 過期訂閱={ExpiredCount}", expiredSubs.Count);
    }
}
