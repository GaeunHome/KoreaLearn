using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Course;
using KoreanLearn.Service.ViewModels.Home;

namespace KoreanLearn.Service.Services.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyList<CourseListViewModel>> GetPublishedCoursesAsync(CancellationToken ct = default);
    Task<PagedResult<CourseListViewModel>> SearchCoursesAsync(string? keyword, int page, int pageSize, CancellationToken ct = default);
    Task<CourseDetailViewModel?> GetCourseDetailAsync(int id, string? userId = null, CancellationToken ct = default);
    Task<HomeViewModel> GetHomeViewModelAsync(CancellationToken ct = default);
}
