namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>系統維護業務邏輯介面（供背景排程服務使用，處理到期訂閱與字卡複習提醒）</summary>
public interface IMaintenanceService
{
    /// <summary>停用所有已到期的訂閱，回傳停用數量</summary>
    Task<int> DeactivateExpiredSubscriptionsAsync(CancellationToken ct = default);

    /// <summary>統計全站待複習的字卡數量</summary>
    Task<int> CountDueFlashcardReviewsAsync(CancellationToken ct = default);
}
