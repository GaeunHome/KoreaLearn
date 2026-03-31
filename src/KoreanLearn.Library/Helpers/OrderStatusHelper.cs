using KoreanLearn.Library.Enums;

namespace KoreanLearn.Library.Helpers;

/// <summary>訂單狀態相關輔助方法</summary>
public static class OrderStatusHelper
{
    /// <summary>取得訂單狀態中文文字</summary>
    public static string GetStatusText(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "待付款",
        OrderStatus.Paid => "已付款",
        OrderStatus.Completed => "已完成",
        OrderStatus.Cancelled => "已取消",
        OrderStatus.Refunded => "已退款",
        _ => "未知"
    };

    /// <summary>取得付款狀態中文文字</summary>
    public static string GetPaymentStatusText(this PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "待付款",
        PaymentStatus.Completed => "已完成",
        PaymentStatus.Failed => "失敗",
        PaymentStatus.Refunded => "已退款",
        _ => "未知"
    };

    /// <summary>取得訂單狀態對應的 Bootstrap badge CSS class</summary>
    public static string GetStatusBadgeClass(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "bg-warning text-dark",
        OrderStatus.Paid => "bg-info",
        OrderStatus.Completed => "bg-success",
        OrderStatus.Cancelled => "bg-secondary",
        OrderStatus.Refunded => "bg-dark",
        _ => "bg-light text-dark"
    };

    /// <summary>檢查訂單是否可取消</summary>
    public static bool CanCancel(this OrderStatus status) => status == OrderStatus.Pending;
}
