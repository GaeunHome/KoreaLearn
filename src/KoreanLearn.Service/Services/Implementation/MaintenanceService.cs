using KoreanLearn.Data;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class MaintenanceService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<MaintenanceService> logger) : IMaintenanceService
{
    public async Task<int> DeactivateExpiredSubscriptionsAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

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

        return expiredSubs.Count;
    }

    public async Task<int> CountDueFlashcardReviewsAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        return await db.FlashcardLogs
            .Where(l => l.NextReviewDate <= DateTime.UtcNow)
            .CountAsync(ct)
            .ConfigureAwait(false);
    }
}
