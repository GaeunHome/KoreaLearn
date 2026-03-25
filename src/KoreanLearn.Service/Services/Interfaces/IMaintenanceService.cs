namespace KoreanLearn.Service.Services.Interfaces;

public interface IMaintenanceService
{
    Task<int> DeactivateExpiredSubscriptionsAsync(CancellationToken ct = default);
    Task<int> CountDueFlashcardReviewsAsync(CancellationToken ct = default);
}
