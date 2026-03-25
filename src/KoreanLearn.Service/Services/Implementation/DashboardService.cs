using KoreanLearn.Data;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class DashboardService(
    IUnitOfWork uow,
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(
        string userId, CancellationToken ct = default)
    {
        var enrollments = await uow.Enrollments.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
        var courses = new List<EnrolledCourseItem>();
        var totalCompleted = 0;
        var totalLessons = 0;

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

        return new StudentDashboardViewModel
        {
            EnrolledCourses = enrollments.Count,
            CompletedLessons = totalCompleted,
            TotalLessons = totalLessons,
            Courses = courses
        };
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        logger.LogInformation("開始載入管理員儀表板資料（平行查詢）");

        // 使用 IDbContextFactory 建立獨立 DbContext 進行平行查詢
        var courseCountTask = CountCoursesAsync(ct);
        var orderCountTask = CountOrdersAsync(ct);
        var revenueTask = CalculateRevenueAsync(ct);
        var recentOrdersTask = GetRecentOrdersAsync(ct);
        var userCountTask = CountUsersAsync(ct);

        await Task.WhenAll(courseCountTask, orderCountTask, revenueTask, recentOrdersTask, userCountTask)
            .ConfigureAwait(false);

        return new AdminDashboardViewModel
        {
            TotalCourses = await courseCountTask.ConfigureAwait(false),
            TotalOrders = await orderCountTask.ConfigureAwait(false),
            TotalRevenue = await revenueTask.ConfigureAwait(false),
            RecentOrders = await recentOrdersTask.ConfigureAwait(false),
            TotalUsers = await userCountTask.ConfigureAwait(false)
        };
    }

    private async Task<int> CountCoursesAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Courses.CountAsync(ct).ConfigureAwait(false);
    }

    private async Task<int> CountOrdersAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Orders.CountAsync(ct).ConfigureAwait(false);
    }

    private async Task<decimal> CalculateRevenueAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Orders
            .Where(o => o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount, ct)
            .ConfigureAwait(false);
    }

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

    private async Task<int> CountUsersAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Users.CountAsync(ct).ConfigureAwait(false);
    }
}
