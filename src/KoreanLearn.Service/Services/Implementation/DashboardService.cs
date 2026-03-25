using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Enums;
using KoreanLearn.Service.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class DashboardService(
    IUnitOfWork uow,
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
        var totalCourses = await uow.Courses.CountAsync(ct).ConfigureAwait(false);
        var totalOrders = await uow.Orders.CountAsync(ct).ConfigureAwait(false);

        var allOrders = await uow.Orders.GetAllAsync(ct).ConfigureAwait(false);
        var revenue = allOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);

        var recentOrders = allOrders.Take(10).Select(o => new RecentOrderItem
        {
            OrderNumber = o.OrderNumber,
            Amount = o.TotalAmount,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt
        }).ToList();

        return new AdminDashboardViewModel
        {
            TotalCourses = totalCourses,
            TotalOrders = totalOrders,
            TotalRevenue = revenue,
            RecentOrders = recentOrders
        };
    }
}
