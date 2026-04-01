using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>訂單業務邏輯實作，處理訂單建立、模擬付款（自動建立選課紀錄）與訂單查詢</summary>
public class OrderService(
    IUnitOfWork uow,
    ILogger<OrderService> logger) : IOrderService
{
    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateOrderAsync(
        string userId, int courseId, CancellationToken ct = default)
    {
        logger.LogInformation("建立訂單 | UserId={UserId} | CourseId={CourseId}", userId, courseId);

        var course = await uow.Courses.GetByIdAsync(courseId, ct).ConfigureAwait(false);
        if (course is null)
        {
            logger.LogWarning("建立訂單失敗：課程不存在 | CourseId={CourseId}", courseId);
            return ServiceResult<int>.Failure("課程不存在");
        }

        // 檢查是否已選過此課程
        if (await uow.Enrollments.IsEnrolledAsync(userId, courseId, ct).ConfigureAwait(false))
        {
            logger.LogWarning("建立訂單失敗：使用者已購買此課程 | UserId={UserId} | CourseId={CourseId}", userId, courseId);
            return ServiceResult<int>.Failure("您已購買此課程");
        }

        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            TotalAmount = course.Price,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            OrderItems = [new OrderItem { CourseId = courseId, Price = course.Price }]
        };

        await uow.Orders.AddAsync(order, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("訂單建立 | OrderId={OrderId} | OrderNumber={OrderNumber}", order.Id, order.OrderNumber);
        return ServiceResult<int>.Success(order.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SimulatePaymentAsync(
        int orderId, string userId, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetWithItemsAsync(orderId, ct).ConfigureAwait(false);
        if (order is null || order.UserId != userId)
        {
            logger.LogWarning("模擬付款失敗：訂單不存在 | OrderId={OrderId} | UserId={UserId}", orderId, userId);
            return ServiceResult.Failure("訂單不存在");
        }
        if (order.Status != OrderStatus.Pending)
        {
            logger.LogWarning("模擬付款失敗：訂單狀態不正確 | OrderId={OrderId} | Status={Status}", orderId, order.Status);
            return ServiceResult.Failure("訂單狀態不正確");
        }

        // 模擬付款成功：更新訂單狀態
        var previousStatus = order.Status;
        order.Status = OrderStatus.Completed;
        order.PaymentStatus = PaymentStatus.Completed;
        uow.Orders.Update(order);

        await CreateEnrollmentsForOrderAsync(order, ct).ConfigureAwait(false);

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("付款完成 | OrderId={OrderId} | TotalAmount={TotalAmount} | {PreviousStatus} → {NewStatus}",
            orderId, order.TotalAmount, previousStatus, order.Status);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<PagedResult<OrderListViewModel>> GetUserOrdersAsync(
        string userId, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Orders.GetByUserIdPagedAsync(userId, page, pageSize, ct).ConfigureAwait(false);
        var items = result.Items.Select(MapToList).ToList();
        return new PagedResult<OrderListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<OrderDetailViewModel?> GetOrderDetailAsync(
        int orderId, string userId, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetWithItemsAsync(orderId, ct).ConfigureAwait(false);
        if (order is null || order.UserId != userId)
        {
            logger.LogWarning("查詢訂單詳情失敗：訂單不存在或無權限 | OrderId={OrderId} | UserId={UserId}", orderId, userId);
            return null;
        }

        return new OrderDetailViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(i => new OrderItemViewModel
            {
                CourseTitle = i.Course?.Title ?? "未知課程",
                Price = i.Price,
                CourseId = i.CourseId
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<PagedResult<OrderListViewModel>> GetAllOrdersPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Orders.GetPagedWithItemsAsync(page, pageSize, ct).ConfigureAwait(false);
        var items = result.Items.Select(MapToList).ToList();
        return new PagedResult<OrderListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CancelOrderAsync(
        int orderId, string userId, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetWithItemsAsync(orderId, ct).ConfigureAwait(false);
        if (order is null || order.UserId != userId)
        {
            logger.LogWarning("取消訂單失敗：訂單不存在 | OrderId={OrderId} | UserId={UserId}", orderId, userId);
            return ServiceResult.Failure("訂單不存在");
        }
        if (order.Status != OrderStatus.Pending)
        {
            logger.LogWarning("取消訂單失敗：僅限待付款狀態 | OrderId={OrderId} | Status={Status}", orderId, order.Status);
            return ServiceResult.Failure("僅限待付款狀態的訂單可取消");
        }

        var previousStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.PaymentStatus = PaymentStatus.Failed;
        uow.Orders.Update(order);

        await RemoveEnrollmentsForOrderAsync(order, ct).ConfigureAwait(false);

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("訂單已取消 | OrderId={OrderId} | {PreviousStatus} → {NewStatus}",
            orderId, previousStatus, order.Status);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<OrderDetailViewModel?> GetOrderDetailForAdminAsync(
        int orderId, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetWithItemsAsync(orderId, ct).ConfigureAwait(false);
        if (order is null)
        {
            logger.LogWarning("後台查詢訂單詳情失敗：訂單不存在 | OrderId={OrderId}", orderId);
            return null;
        }

        return new OrderDetailViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            CreatedAt = order.CreatedAt,
            UserDisplayName = order.User?.DisplayName,
            UserEmail = order.User?.Email,
            Items = order.OrderItems.Select(i => new OrderItemViewModel
            {
                CourseTitle = i.Course?.Title ?? "未知課程",
                Price = i.Price,
                CourseId = i.CourseId
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateOrderStatusAsync(
        int orderId, string newStatus, CancellationToken ct = default)
    {
        if (!Enum.TryParse<OrderStatus>(newStatus, ignoreCase: true, out var status))
        {
            logger.LogWarning("更新訂單狀態失敗：無效的狀態值 | OrderId={OrderId} | NewStatus={NewStatus}", orderId, newStatus);
            return ServiceResult.Failure("無效的訂單狀態");
        }

        var order = await uow.Orders.GetWithItemsAsync(orderId, ct).ConfigureAwait(false);
        if (order is null)
        {
            logger.LogWarning("更新訂單狀態失敗：訂單不存在 | OrderId={OrderId}", orderId);
            return ServiceResult.Failure("訂單不存在");
        }

        var previousStatus = order.Status;
        order.Status = status;

        if (status == OrderStatus.Completed)
        {
            order.PaymentStatus = PaymentStatus.Completed;
            order.PaidAt = DateTime.UtcNow;
            await CreateEnrollmentsForOrderAsync(order, ct).ConfigureAwait(false);
        }
        else if (status == OrderStatus.Cancelled)
        {
            order.PaymentStatus = PaymentStatus.Failed;
            await RemoveEnrollmentsForOrderAsync(order, ct).ConfigureAwait(false);
        }

        uow.Orders.Update(order);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("訂單狀態更新 | OrderId={OrderId} | {PreviousStatus} → {NewStatus}",
            orderId, previousStatus, status);
        return ServiceResult.Success();
    }

    /// <summary>為訂單中的每筆項目建立選課紀錄（已存在則略過）</summary>
    private async Task CreateEnrollmentsForOrderAsync(Order order, CancellationToken ct)
    {
        foreach (var item in order.OrderItems)
        {
            var existing = await uow.Enrollments.GetByUserAndCourseAsync(order.UserId, item.CourseId, ct).ConfigureAwait(false);
            if (existing is null)
            {
                await uow.Enrollments.AddAsync(new Enrollment
                {
                    UserId = order.UserId,
                    CourseId = item.CourseId,
                    Status = EnrollmentStatus.Active
                }, ct).ConfigureAwait(false);
                logger.LogInformation("選課紀錄自動建立 | UserId={UserId} | CourseId={CourseId}", order.UserId, item.CourseId);
            }
        }
    }

    /// <summary>移除訂單中每筆項目的選課紀錄（不存在則略過）</summary>
    private async Task RemoveEnrollmentsForOrderAsync(Order order, CancellationToken ct)
    {
        foreach (var item in order.OrderItems)
        {
            var enrollment = await uow.Enrollments.GetByUserAndCourseAsync(order.UserId, item.CourseId, ct).ConfigureAwait(false);
            if (enrollment is not null)
            {
                uow.Enrollments.Remove(enrollment);
                logger.LogInformation("選課紀錄因訂單取消而移除 | UserId={UserId} | CourseId={CourseId}", order.UserId, item.CourseId);
            }
        }
    }

    /// <summary>將 Order Entity 轉換為列表 ViewModel</summary>
    private static OrderListViewModel MapToList(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        TotalAmount = o.TotalAmount,
        Status = o.Status.ToString(),
        PaymentStatus = o.PaymentStatus.ToString(),
        CreatedAt = o.CreatedAt,
        CourseName = o.OrderItems.FirstOrDefault()?.Course?.Title
    };
}
