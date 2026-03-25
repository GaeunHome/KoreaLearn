namespace KoreanLearn.Library.Enums;

/// <summary>選課狀態</summary>
public enum EnrollmentStatus
{
    /// <summary>有效（學習中）</summary>
    Active    = 0,

    /// <summary>已完成</summary>
    Completed = 1,

    /// <summary>已過期</summary>
    Expired   = 2,

    /// <summary>已取消</summary>
    Cancelled = 3
}
