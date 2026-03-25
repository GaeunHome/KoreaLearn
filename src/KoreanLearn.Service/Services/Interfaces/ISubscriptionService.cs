using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionPlanViewModel>> GetActivePlansAsync(CancellationToken ct = default);
    Task<UserSubscriptionViewModel?> GetUserSubscriptionAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult> SubscribeAsync(string userId, int planId, CancellationToken ct = default);
    Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken ct = default);

    // Admin
    Task<PagedResult<SubscriptionPlanViewModel>> GetPlansPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<int>> CreatePlanAsync(string name, string? description, decimal monthlyPrice, int durationMonths, CancellationToken ct = default);
    Task<ServiceResult> DeletePlanAsync(int id, CancellationToken ct = default);
}

public class SubscriptionPlanViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int DurationMonths { get; set; }
    public bool IsActive { get; set; }
}

public class UserSubscriptionViewModel
{
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int RemainingDays => Math.Max(0, (EndDate - DateTime.UtcNow).Days);
}
