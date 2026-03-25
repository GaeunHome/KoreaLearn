using KoreanLearn.Library.Helpers;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>訂單業務邏輯介面（建立訂單、模擬付款、查詢訂單）</summary>
public interface IOrderService
{
    /// <summary>為使用者建立課程訂單，回傳新訂單 ID</summary>
    Task<ServiceResult<int>> CreateOrderAsync(string userId, int courseId, CancellationToken ct = default);

    /// <summary>模擬付款成功（更新訂單狀態並自動建立選課紀錄）</summary>
    Task<ServiceResult> SimulatePaymentAsync(int orderId, string userId, CancellationToken ct = default);

    /// <summary>取得使用者的訂單列表（分頁）</summary>
    Task<PagedResult<OrderListViewModel>> GetUserOrdersAsync(string userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得訂單詳情（僅限訂單擁有者）</summary>
    Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId, string userId, CancellationToken ct = default);

    /// <summary>取得所有訂單分頁列表（後台管理用）</summary>
    Task<PagedResult<OrderListViewModel>> GetAllOrdersPagedAsync(int page, int pageSize, CancellationToken ct = default);
}

/// <summary>訂單列表項目 ViewModel</summary>
public class OrderListViewModel
{
    /// <summary>訂單 ID</summary>
    public int Id { get; set; }

    /// <summary>訂單編號</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>訂單總金額</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>訂單狀態</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>付款狀態</summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>課程名稱（取自第一筆訂單項目）</summary>
    public string? CourseName { get; set; }
}

/// <summary>訂單詳情 ViewModel（含訂單項目明細）</summary>
public class OrderDetailViewModel
{
    /// <summary>訂單 ID</summary>
    public int Id { get; set; }

    /// <summary>訂單編號</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>訂單總金額</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>訂單狀態</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>付款狀態</summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>訂單項目明細</summary>
    public IReadOnlyList<OrderItemViewModel> Items { get; set; } = [];
}

/// <summary>訂單項目 ViewModel</summary>
public class OrderItemViewModel
{
    /// <summary>課程標題</summary>
    public string CourseTitle { get; set; } = string.Empty;

    /// <summary>價格</summary>
    public decimal Price { get; set; }

    /// <summary>課程 ID</summary>
    public int CourseId { get; set; }
}
