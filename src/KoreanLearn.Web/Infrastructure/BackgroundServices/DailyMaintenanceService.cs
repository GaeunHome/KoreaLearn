using KoreanLearn.Service.Services.Interfaces;

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
        var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();

        var dueCards = await maintenanceService.CountDueFlashcardReviewsAsync(ct);
        logger.LogInformation("今日待複習字卡總數：{Count}", dueCards);

        var expiredCount = await maintenanceService.DeactivateExpiredSubscriptionsAsync(ct);
        logger.LogInformation("DailyMaintenanceService 完成 | 過期訂閱={ExpiredCount}", expiredCount);
    }
}
