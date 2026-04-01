using KoreanLearn.Data;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>儀表板業務邏輯實作，管理員儀表板使用 IDbContextFactory 進行平行查詢以提升效能</summary>
public class DashboardService(
    IUnitOfWork uow,
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<DashboardService> logger) : IDashboardService
{
    /// <inheritdoc />
    public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(
        string userId, CancellationToken ct = default)
    {
        logger.LogInformation("載入學生儀表板 | UserId={UserId}", userId);
        var enrollments = await uow.Enrollments.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
        var courses = new List<EnrolledCourseItem>();
        var totalCompleted = 0;
        var totalLessons = 0;

        // 逐一計算每門已選課程的學習進度
        foreach (var enrollment in enrollments)
        {
            var course = await uow.Courses.GetWithSectionsAndLessonsAsync(enrollment.CourseId, ct).ConfigureAwait(false);
            if (course is null) continue;

            var lessonCount = course.Sections.SelectMany(s => s.Lessons).Count();
            var progresses = await uow.Progresses.GetByUserAndCourseAsync(userId, course.Id, ct).ConfigureAwait(false);
            var completed = progresses.Count(p => p.IsCompleted);

            totalCompleted += completed;
            totalLessons += lessonCount;

            courses.Add(new EnrolledCourseItem
            {
                CourseId = course.Id,
                Title = course.Title,
                CoverImageUrl = course.CoverImageUrl,
                ProgressPercent = lessonCount > 0 ? completed * 100 / lessonCount : 0,
                CompletedLessons = completed,
                TotalLessons = lessonCount
            });
        }

        logger.LogInformation("學生儀表板載入完成 | UserId={UserId} | EnrolledCourses={Count} | CompletedLessons={Completed}/{Total}",
            userId, enrollments.Count, totalCompleted, totalLessons);

        return new StudentDashboardViewModel
        {
            EnrolledCourses = enrollments.Count,
            CompletedLessons = totalCompleted,
            TotalLessons = totalLessons,
            Courses = courses
        };
    }

    /// <inheritdoc />
    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        logger.LogInformation("開始載入管理員儀表板資料（平行查詢）");

        // 使用 IDbContextFactory 建立獨立 DbContext 進行平行查詢（DbContext 非 thread-safe）
        var courseCountTask = CountCoursesAsync(ct);
        var orderCountTask = CountOrdersAsync(ct);
        var revenueTask = CalculateRevenueAsync(ct);
        var recentOrdersTask = GetRecentOrdersAsync(ct);
        var userCountTask = CountUsersAsync(ct);

        await Task.WhenAll(courseCountTask, orderCountTask, revenueTask, recentOrdersTask, userCountTask)
            .ConfigureAwait(false);

        var result = new AdminDashboardViewModel
        {
            TotalCourses = await courseCountTask.ConfigureAwait(false),
            TotalOrders = await orderCountTask.ConfigureAwait(false),
            TotalRevenue = await revenueTask.ConfigureAwait(false),
            RecentOrders = await recentOrdersTask.ConfigureAwait(false),
            TotalUsers = await userCountTask.ConfigureAwait(false)
        };

        logger.LogInformation("管理員儀表板載入完成 | TotalCourses={Courses} | TotalOrders={Orders} | TotalRevenue={Revenue} | TotalUsers={Users}",
            result.TotalCourses, result.TotalOrders, result.TotalRevenue, result.TotalUsers);

        return result;
    }

    /// <summary>以獨立 DbContext 計算課程總數</summary>
    private async Task<int> CountCoursesAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Courses.CountAsync(ct).ConfigureAwait(false);
    }

    /// <summary>以獨立 DbContext 計算訂單總數</summary>
    private async Task<int> CountOrdersAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Orders.CountAsync(ct).ConfigureAwait(false);
    }

    /// <summary>以獨立 DbContext 計算已完成訂單的總營收</summary>
    private async Task<decimal> CalculateRevenueAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Orders
            .Where(o => o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount, ct)
            .ConfigureAwait(false);
    }

    /// <summary>以獨立 DbContext 取得最近 10 筆訂單</summary>
    private async Task<IReadOnlyList<RecentOrderItem>> GetRecentOrdersAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderItem
            {
                OrderNumber = o.OrderNumber,
                Amount = o.TotalAmount,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <summary>以獨立 DbContext 計算使用者總數</summary>
    private async Task<int> CountUsersAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Users.CountAsync(ct).ConfigureAwait(false);
    }
}
