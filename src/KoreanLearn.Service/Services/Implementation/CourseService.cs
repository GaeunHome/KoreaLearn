using AutoMapper;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Course;
using KoreanLearn.Service.ViewModels.Home;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class CourseService(
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<CourseService> logger) : ICourseService
{
    public async Task<IReadOnlyList<CourseListViewModel>> GetPublishedCoursesAsync(
        CancellationToken ct = default)
    {
        var courses = await uow.Courses.GetPublishedAsync(ct).ConfigureAwait(false);
        return mapper.Map<IReadOnlyList<CourseListViewModel>>(courses);
    }

    public async Task<PagedResult<CourseListViewModel>> SearchCoursesAsync(
        string? keyword, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Courses.SearchAsync(keyword, page, pageSize, ct).ConfigureAwait(false);
        var items = mapper.Map<IReadOnlyList<CourseListViewModel>>(result.Items);
        return new PagedResult<CourseListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task<CourseDetailViewModel?> GetCourseDetailAsync(
        int id, CancellationToken ct = default)
    {
        var course = await uow.Courses.GetWithSectionsAndLessonsAsync(id, ct).ConfigureAwait(false);
        if (course is null || !course.IsPublished)
            return null;

        return mapper.Map<CourseDetailViewModel>(course);
    }

    public async Task<HomeViewModel> GetHomeViewModelAsync(CancellationToken ct = default)
    {
        var courses = await uow.Courses.GetPublishedAsync(ct).ConfigureAwait(false);
        var announcements = await uow.Announcements.GetActiveAsync(ct).ConfigureAwait(false);

        return new HomeViewModel
        {
            FeaturedCourses = mapper.Map<IReadOnlyList<CourseListViewModel>>(courses),
            Announcements = mapper.Map<IReadOnlyList<AnnouncementViewModel>>(announcements)
        };
    }
}
