namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>儀表板業務邏輯介面（提供學生與管理員各自的儀表板資料）</summary>
public interface IDashboardService
{
    /// <summary>取得學生儀表板資料（已選課程、學習進度）</summary>
    Task<StudentDashboardViewModel> GetStudentDashboardAsync(string userId, CancellationToken ct = default);

    /// <summary>取得管理員儀表板資料（使用 IDbContextFactory 平行查詢統計數據）</summary>
    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken ct = default);
}

/// <summary>學生儀表板 ViewModel</summary>
public class StudentDashboardViewModel
{
    /// <summary>已選課程數</summary>
    public int EnrolledCourses { get; set; }

    /// <summary>已完成單元數</summary>
    public int CompletedLessons { get; set; }

    /// <summary>全部單元數</summary>
    public int TotalLessons { get; set; }

    /// <summary>已選課程明細（含進度百分比）</summary>
    public IReadOnlyList<EnrolledCourseItem> Courses { get; set; } = [];
}

/// <summary>已選課程項目（用於學生儀表板）</summary>
public class EnrolledCourseItem
{
    /// <summary>課程 ID</summary>
    public int CourseId { get; set; }

    /// <summary>課程標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>封面圖片網址</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>學習進度百分比</summary>
    public int ProgressPercent { get; set; }

    /// <summary>已完成單元數</summary>
    public int CompletedLessons { get; set; }

    /// <summary>該課程總單元數</summary>
    public int TotalLessons { get; set; }
}

/// <summary>管理員儀表板 ViewModel</summary>
public class AdminDashboardViewModel
{
    /// <summary>總使用者數</summary>
    public int TotalUsers { get; set; }

    /// <summary>總課程數</summary>
    public int TotalCourses { get; set; }

    /// <summary>總訂單數</summary>
    public int TotalOrders { get; set; }

    /// <summary>總營收</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>近期訂單列表</summary>
    public IReadOnlyList<RecentOrderItem> RecentOrders { get; set; } = [];
}

/// <summary>近期訂單項目（用於管理員儀表板）</summary>
public class RecentOrderItem
{
    /// <summary>訂單編號</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>訂單金額</summary>
    public decimal Amount { get; set; }

    /// <summary>訂單狀態</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }
}
