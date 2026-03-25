using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>訂閱制方案業務邏輯實作，處理訂閱方案管理與使用者訂閱流程</summary>
public class SubscriptionService(
    IUnitOfWork uow,
    ILogger<SubscriptionService> logger) : ISubscriptionService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SubscriptionPlanViewModel>> GetActivePlansAsync(CancellationToken ct = default)
    {
        var plans = await uow.SubscriptionPlans.GetActivePlansAsync(ct).ConfigureAwait(false);
        return plans.Select(p => new SubscriptionPlanViewModel
        {
            Id = p.Id, Name = p.Name, Description = p.Description,
            MonthlyPrice = p.MonthlyPrice, DurationMonths = p.DurationMonths, IsActive = p.IsActive
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<UserSubscriptionViewModel?> GetUserSubscriptionAsync(string userId, CancellationToken ct = default)
    {
        var sub = await uow.UserSubscriptions.GetActiveByUserAsync(userId, ct).ConfigureAwait(false);
        if (sub is null) return null;
        return new UserSubscriptionViewModel
        {
            PlanName = sub.Plan.Name, StartDate = sub.StartDate,
            EndDate = sub.EndDate, IsActive = sub.IsActive
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SubscribeAsync(string userId, int planId, CancellationToken ct = default)
    {
        var plan = await uow.SubscriptionPlans.GetByIdAsync(planId, ct).ConfigureAwait(false);
        if (plan is null || !plan.IsActive) return ServiceResult.Failure("方案不存在或已停用");

        var existing = await uow.UserSubscriptions.GetActiveByUserAsync(userId, ct).ConfigureAwait(false);
        if (existing is not null) return ServiceResult.Failure("您已有有效訂閱");

        var sub = new UserSubscription
        {
            UserId = userId, PlanId = planId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(plan.DurationMonths),
            IsActive = true
        };
        await uow.UserSubscriptions.AddAsync(sub, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("用戶訂閱成功 | UserId={UserId} | PlanId={PlanId}", userId, planId);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken ct = default)
    {
        var sub = await uow.UserSubscriptions.GetActiveByUserAsync(userId, ct).ConfigureAwait(false);
        return sub is not null;
    }

    /// <inheritdoc />
    public async Task<PagedResult<SubscriptionPlanViewModel>> GetPlansPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.SubscriptionPlans.GetPagedAsync(page, pageSize, ct).ConfigureAwait(false);
        var items = result.Items.Select(p => new SubscriptionPlanViewModel
        {
            Id = p.Id, Name = p.Name, Description = p.Description,
            MonthlyPrice = p.MonthlyPrice, DurationMonths = p.DurationMonths, IsActive = p.IsActive
        }).ToList();
        return new PagedResult<SubscriptionPlanViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreatePlanAsync(
        string name, string? description, decimal monthlyPrice, int durationMonths, CancellationToken ct = default)
    {
        var plan = new SubscriptionPlan
        {
            Name = name, Description = description,
            MonthlyPrice = monthlyPrice, DurationMonths = durationMonths, IsActive = true
        };
        await uow.SubscriptionPlans.AddAsync(plan, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult<int>.Success(plan.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeletePlanAsync(int id, CancellationToken ct = default)
    {
        var plan = await uow.SubscriptionPlans.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (plan is null) return ServiceResult.Failure("方案不存在");
        uow.SubscriptionPlans.Remove(plan);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }
}
