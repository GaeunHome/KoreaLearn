using MapsterMapper;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Course;
using KoreanLearn.Service.ViewModels.Home;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>前台課程業務邏輯實作，處理課程瀏覽、搜尋與首頁資料載入</summary>
public class CourseService(
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<CourseService> logger) : ICourseService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<CourseListViewModel>> GetPublishedCoursesAsync(
        CancellationToken ct = default)
    {
        logger.LogInformation("查詢已發佈課程列表");
        var courses = await uow.Courses.GetPublishedAsync(ct).ConfigureAwait(false);
        logger.LogInformation("取得 {Count} 門已發佈課程", courses.Count);
        return mapper.Map<IReadOnlyList<CourseListViewModel>>(courses);
    }

    /// <inheritdoc />
    public async Task<PagedResult<CourseListViewModel>> SearchCoursesAsync(
        string? keyword, int page, int pageSize, CancellationToken ct = default)
    {
        logger.LogInformation("搜尋課程 | Keyword={Keyword} | Page={Page} | PageSize={PageSize}",
            keyword ?? "(空)", page, pageSize);
        var result = await uow.Courses.SearchAsync(keyword, page, pageSize, ct).ConfigureAwait(false);
        logger.LogInformation("搜尋結果 | TotalCount={TotalCount} | ReturnedCount={Count}",
            result.TotalCount, result.Items.Count);
        var items = mapper.Map<IReadOnlyList<CourseListViewModel>>(result.Items);
        return new PagedResult<CourseListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<CourseDetailViewModel?> GetCourseDetailAsync(
        int id, string? userId = null, CancellationToken ct = default)
    {
        logger.LogInformation("查詢課程詳情 | CourseId={CourseId}", id);
        var course = await uow.Courses.GetWithSectionsAndLessonsAsync(id, ct).ConfigureAwait(false);

        if (course is null)
        {
            logger.LogWarning("課程不存在 | CourseId={CourseId}", id);
            return null;
        }

        if (!course.IsPublished)
        {
            logger.LogWarning("課程未發佈 | CourseId={CourseId} | Title={Title}", id, course.Title);
            return null;
        }

        logger.LogInformation("取得課程詳情 | CourseId={CourseId} | Title={Title} | Sections={SectionCount}",
            id, course.Title, course.Sections.Count);
        var vm = mapper.Map<CourseDetailViewModel>(course);
        vm.TeacherName = course.Teacher?.DisplayName;

        // 若使用者已登入，填入選課狀態與學習進度
        if (!string.IsNullOrEmpty(userId))
        {
            vm.IsEnrolled = await uow.Enrollments.HasActiveAccessAsync(userId, id, ct).ConfigureAwait(false);

            if (vm.IsEnrolled)
            {
                var progresses = await uow.Progresses.GetByUserAndCourseAsync(userId, id, ct).ConfigureAwait(false);
                var completedLessonIds = progresses.Where(p => p.IsCompleted).Select(p => p.LessonId).ToHashSet();
                foreach (var sec in vm.Sections)
                {
                    foreach (var lesson in sec.Lessons)
                    {
                        lesson.IsCompleted = completedLessonIds.Contains(lesson.Id);
                    }
                }
            }
        }

        return vm;
    }

    /// <inheritdoc />
    public async Task<HomeViewModel> GetHomeViewModelAsync(CancellationToken ct = default)
    {
        logger.LogInformation("載入首頁資料");
        var courses = await uow.Courses.GetPublishedAsync(ct).ConfigureAwait(false);
        var announcements = await uow.Announcements.GetActiveAsync(ct).ConfigureAwait(false);
        logger.LogInformation("首頁資料 | Courses={CourseCount} | Announcements={AnnCount}",
            courses.Count, announcements.Count);

        return new HomeViewModel
        {
            FeaturedCourses = mapper.Map<IReadOnlyList<CourseListViewModel>>(courses),
            Announcements = mapper.Map<IReadOnlyList<AnnouncementViewModel>>(announcements)
        };
    }
}
