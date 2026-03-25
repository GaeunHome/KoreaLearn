using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<int>> CreateOrderAsync(string userId, int courseId, CancellationToken ct = default);
    Task<ServiceResult> SimulatePaymentAsync(int orderId, string userId, CancellationToken ct = default);
    Task<PagedResult<OrderListViewModel>> GetUserOrdersAsync(string userId, int page, int pageSize, CancellationToken ct = default);
    Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId, string userId, CancellationToken ct = default);
    Task<PagedResult<OrderListViewModel>> GetAllOrdersPagedAsync(int page, int pageSize, CancellationToken ct = default);
}

public class OrderListViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CourseName { get; set; }
}

public class OrderDetailViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<OrderItemViewModel> Items { get; set; } = [];
}

public class OrderItemViewModel
{
    public string CourseTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CourseId { get; set; }
}
