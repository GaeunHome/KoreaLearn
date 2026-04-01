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
        logger.LogInformation("查詢啟用中的訂閱方案");
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
        logger.LogInformation("查詢使用者訂閱狀態 | UserId={UserId}", userId);
        var sub = await uow.UserSubscriptions.GetActiveByUserAsync(userId, ct).ConfigureAwait(false);
        if (sub is null) return null;
        return new UserSubscriptionViewModel
        {
            PlanName = sub.Plan?.Name ?? "未知方案", StartDate = sub.StartDate,
            EndDate = sub.EndDate, IsActive = sub.IsActive
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SubscribeAsync(string userId, int planId, CancellationToken ct = default)
    {
        logger.LogInformation("訂閱申請 | UserId={UserId} | PlanId={PlanId}", userId, planId);
        var plan = await uow.SubscriptionPlans.GetByIdAsync(planId, ct).ConfigureAwait(false);
        if (plan is null || !plan.IsActive)
        {
            logger.LogWarning("訂閱失敗：方案不存在或已停用 | PlanId={PlanId}", planId);
            return ServiceResult.Failure("方案不存在或已停用");
        }

        var existing = await uow.UserSubscriptions.GetActiveByUserAsync(userId, ct).ConfigureAwait(false);
        if (existing is not null)
        {
            logger.LogWarning("訂閱失敗：使用者已有有效訂閱 | UserId={UserId}", userId);
            return ServiceResult.Failure("您已有有效訂閱");
        }

        var sub = new UserSubscription
        {
            UserId = userId, PlanId = planId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(plan.DurationMonths),
            IsActive = true
        };
        await uow.UserSubscriptions.AddAsync(sub, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("用戶訂閱成功 | UserId={UserId} | PlanId={PlanId} | EndDate={EndDate}",
            userId, planId, sub.EndDate);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken ct = default)
    {
        logger.LogInformation("檢查使用者是否有有效訂閱 | UserId={UserId}", userId);
        var sub = await uow.UserSubscriptions.GetActiveByUserAsync(userId, ct).ConfigureAwait(false);
        return sub is not null;
    }

    /// <inheritdoc />
    public async Task<PagedResult<SubscriptionPlanViewModel>> GetPlansPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        logger.LogInformation("取得訂閱方案列表 | Page={Page} | PageSize={PageSize}", page, pageSize);
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
        logger.LogInformation("訂閱方案建立成功 | PlanId={PlanId} | Name={Name} | MonthlyPrice={MonthlyPrice}",
            plan.Id, name, monthlyPrice);
        return ServiceResult<int>.Success(plan.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeletePlanAsync(int id, CancellationToken ct = default)
    {
        var plan = await uow.SubscriptionPlans.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (plan is null)
        {
            logger.LogWarning("刪除訂閱方案失敗：方案不存在 | PlanId={PlanId}", id);
            return ServiceResult.Failure("方案不存在");
        }
        uow.SubscriptionPlans.Remove(plan);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("訂閱方案刪除成功 | PlanId={PlanId} | Name={Name}", id, plan.Name);
        return ServiceResult.Success();
    }
}
