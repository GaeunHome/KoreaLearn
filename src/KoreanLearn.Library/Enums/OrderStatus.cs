namespace KoreanLearn.Library.Enums;

/// <summary>訂單狀態</summary>
public enum OrderStatus
{
    /// <summary>待處理</summary>
    Pending   = 0,

    /// <summary>已付款</summary>
    Paid      = 1,

    /// <summary>已完成</summary>
    Completed = 2,

    /// <summary>已取消</summary>
    Cancelled = 3,

    /// <summary>已退款</summary>
    Refunded  = 4
}
