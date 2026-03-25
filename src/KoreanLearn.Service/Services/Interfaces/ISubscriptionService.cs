using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>訂閱制方案業務邏輯介面（方案查詢、使用者訂閱、後台管理）</summary>
public interface ISubscriptionService
{
    /// <summary>取得所有啟用中的訂閱方案</summary>
    Task<IReadOnlyList<SubscriptionPlanViewModel>> GetActivePlansAsync(CancellationToken ct = default);

    /// <summary>取得使用者目前的有效訂閱資訊</summary>
    Task<UserSubscriptionViewModel?> GetUserSubscriptionAsync(string userId, CancellationToken ct = default);

    /// <summary>使用者訂閱指定方案</summary>
    Task<ServiceResult> SubscribeAsync(string userId, int planId, CancellationToken ct = default);

    /// <summary>檢查使用者是否擁有有效訂閱</summary>
    Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken ct = default);

    // ── 後台管理 ──

    /// <summary>取得訂閱方案分頁列表（後台管理用）</summary>
    Task<PagedResult<SubscriptionPlanViewModel>> GetPlansPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>建立新訂閱方案，回傳方案 ID</summary>
    Task<ServiceResult<int>> CreatePlanAsync(string name, string? description, decimal monthlyPrice, int durationMonths, CancellationToken ct = default);

    /// <summary>軟刪除訂閱方案</summary>
    Task<ServiceResult> DeletePlanAsync(int id, CancellationToken ct = default);
}

/// <summary>訂閱方案 ViewModel</summary>
public class SubscriptionPlanViewModel
{
    /// <summary>方案 ID</summary>
    public int Id { get; set; }

    /// <summary>方案名稱</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>方案說明</summary>
    public string? Description { get; set; }

    /// <summary>月費</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>方案期限（月數）</summary>
    public int DurationMonths { get; set; }

    /// <summary>是否啟用</summary>
    public bool IsActive { get; set; }
}

/// <summary>使用者訂閱狀態 ViewModel</summary>
public class UserSubscriptionViewModel
{
    /// <summary>方案名稱</summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>訂閱起始日</summary>
    public DateTime StartDate { get; set; }

    /// <summary>訂閱到期日</summary>
    public DateTime EndDate { get; set; }

    /// <summary>是否仍有效</summary>
    public bool IsActive { get; set; }

    /// <summary>剩餘天數</summary>
    public int RemainingDays => Math.Max(0, (EndDate - DateTime.UtcNow).Days);
}
