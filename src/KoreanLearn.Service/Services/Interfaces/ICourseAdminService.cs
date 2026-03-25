using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;

namespace KoreanLearn.Service.Services.Interfaces;

public interface ICourseAdminService
{
    // Course
    Task<PagedResult<CourseAdminListViewModel>> GetCoursesPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<CourseDetailAdminViewModel?> GetCourseDetailAsync(int id, CancellationToken ct = default);
    Task<EditCourseViewModel?> GetCourseForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateCourseAsync(CreateCourseViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> UpdateCourseAsync(EditCourseViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> DeleteCourseAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> UpdateCourseImageAsync(int courseId, string imageUrl, CancellationToken ct = default);

    // Section
    Task<SectionFormViewModel?> GetSectionForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateSectionAsync(SectionFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> UpdateSectionAsync(SectionFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> DeleteSectionAsync(int id, CancellationToken ct = default);

    // Lesson
    Task<LessonFormViewModel?> GetLessonForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateLessonAsync(LessonFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> UpdateLessonAsync(LessonFormViewModel vm, CancellationToken ct = default);
    Task<ServiceResult> DeleteLessonAsync(int id, CancellationToken ct = default);
}
