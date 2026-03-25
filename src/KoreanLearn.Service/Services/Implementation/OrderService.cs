using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class OrderService(
    IUnitOfWork uow,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<ServiceResult<int>> CreateOrderAsync(
        string userId, int courseId, CancellationToken ct = default)
    {
        var course = await uow.Courses.GetByIdAsync(courseId, ct).ConfigureAwait(false);
        if (course is null) return ServiceResult<int>.Failure("課程不存在");

        // Check if already enrolled
        if (await uow.Enrollments.IsEnrolledAsync(userId, courseId, ct).ConfigureAwait(false))
            return ServiceResult<int>.Failure("您已購買此課程");

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

    public async Task<ServiceResult> SimulatePaymentAsync(
        int orderId, string userId, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetWithItemsAsync(orderId, ct).ConfigureAwait(false);
        if (order is null || order.UserId != userId)
            return ServiceResult.Failure("訂單不存在");
        if (order.Status != OrderStatus.Pending)
            return ServiceResult.Failure("訂單狀態不正確");

        order.Status = OrderStatus.Completed;
        order.PaymentStatus = PaymentStatus.Completed;
        uow.Orders.Update(order);

        // Create enrollments for each course in the order
        foreach (var item in order.OrderItems)
        {
            var existing = await uow.Enrollments.GetByUserAndCourseAsync(userId, item.CourseId, ct).ConfigureAwait(false);
            if (existing is null)
            {
                await uow.Enrollments.AddAsync(new Enrollment
                {
                    UserId = userId,
                    CourseId = item.CourseId,
                    Status = EnrollmentStatus.Active
                }, ct).ConfigureAwait(false);
            }
        }

        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("付款完成 | OrderId={OrderId}", orderId);
        return ServiceResult.Success();
    }

    public async Task<PagedResult<OrderListViewModel>> GetUserOrdersAsync(
        string userId, int page, int pageSize, CancellationToken ct = default)
    {
        var orders = await uow.Orders.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
        var total = orders.Count;
        var items = orders.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToList).ToList();
        return new PagedResult<OrderListViewModel>(items, total, page, pageSize);
    }

    public async Task<OrderDetailViewModel?> GetOrderDetailAsync(
        int orderId, string userId, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetWithItemsAsync(orderId, ct).ConfigureAwait(false);
        if (order is null || order.UserId != userId) return null;

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

    public async Task<PagedResult<OrderListViewModel>> GetAllOrdersPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Orders.GetPagedWithItemsAsync(page, pageSize, ct).ConfigureAwait(false);
        var items = result.Items.Select(MapToList).ToList();
        return new PagedResult<OrderListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

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
