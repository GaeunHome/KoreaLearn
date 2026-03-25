using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;
using KoreanLearn.Service.ViewModels.Teacher;

namespace KoreanLearn.Service.Services.Interfaces;

public interface ITeacherCourseService
{
    Task<TeacherDashboardViewModel> GetDashboardAsync(string teacherId, CancellationToken ct = default);
    Task<PagedResult<CourseAdminListViewModel>> GetTeacherCoursesPagedAsync(string teacherId, int page, int pageSize, CancellationToken ct = default);
    Task<CourseDetailAdminViewModel?> GetCourseDetailAsync(int id, string teacherId, CancellationToken ct = default);
    Task<EditCourseViewModel?> GetCourseForEditAsync(int id, string teacherId, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateCourseAsync(CreateCourseViewModel vm, string teacherId, CancellationToken ct = default);
    Task<ServiceResult> UpdateCourseAsync(EditCourseViewModel vm, string teacherId, CancellationToken ct = default);
    Task<ServiceResult> DeleteCourseAsync(int id, string teacherId, CancellationToken ct = default);
    Task<ServiceResult> UpdateCourseImageAsync(int courseId, string imageUrl, string teacherId, CancellationToken ct = default);

    // Section — 透過課程所有權驗證
    Task<SectionFormViewModel?> GetSectionForEditAsync(int id, string teacherId, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateSectionAsync(SectionFormViewModel vm, string teacherId, CancellationToken ct = default);
    Task<ServiceResult> UpdateSectionAsync(SectionFormViewModel vm, string teacherId, CancellationToken ct = default);
    Task<ServiceResult> DeleteSectionAsync(int id, string teacherId, CancellationToken ct = default);

    // Lesson — 透過課程所有權驗證
    Task<LessonFormViewModel?> GetLessonForEditAsync(int id, string teacherId, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateLessonAsync(LessonFormViewModel vm, string teacherId, CancellationToken ct = default);
    Task<ServiceResult> UpdateLessonAsync(LessonFormViewModel vm, string teacherId, CancellationToken ct = default);
    Task<ServiceResult> DeleteLessonAsync(int id, string teacherId, CancellationToken ct = default);
}
