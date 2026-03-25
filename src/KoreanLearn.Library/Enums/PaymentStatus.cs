namespace KoreanLearn.Library.Enums;

/// <summary>付款狀態</summary>
public enum PaymentStatus
{
    /// <summary>待付款</summary>
    Pending   = 0,

    /// <summary>已完成</summary>
    Completed = 1,

    /// <summary>付款失敗</summary>
    Failed    = 2,

    /// <summary>已退款</summary>
    Refunded  = 3
}
