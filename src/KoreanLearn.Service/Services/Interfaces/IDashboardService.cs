namespace KoreanLearn.Service.Services.Interfaces;

public interface IDashboardService
{
    Task<StudentDashboardViewModel> GetStudentDashboardAsync(string userId, CancellationToken ct = default);
    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken ct = default);
}

public class StudentDashboardViewModel
{
    public int EnrolledCourses { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public IReadOnlyList<EnrolledCourseItem> Courses { get; set; } = [];
}

public class EnrolledCourseItem
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public int ProgressPercent { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
}

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public IReadOnlyList<RecentOrderItem> RecentOrders { get; set; } = [];
}

public class RecentOrderItem
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
