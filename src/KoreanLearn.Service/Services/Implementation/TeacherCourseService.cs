using MapsterMapper;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;
using KoreanLearn.Service.ViewModels.Teacher;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>教師課程管理業務邏輯實作，所有操作皆先驗證教師對課程的所有權，再委派給 CourseAdminService 執行</summary>
public class TeacherCourseService(
    IUnitOfWork uow,
    IMapper mapper,
    ICourseAdminService adminService,
    ILogger<TeacherCourseService> logger) : ITeacherCourseService
{
    // ── 儀表板 ─────────────────────────────────────

    /// <inheritdoc />
    public async Task<TeacherDashboardViewModel> GetDashboardAsync(
        string teacherId, CancellationToken ct = default)
    {
        var courses = await uow.Courses.GetByTeacherIdPagedAsync(teacherId, 1, 100, ct).ConfigureAwait(false);
        var courseIds = courses.Items.Select(c => c.Id).ToList();
        var totalStudents = await uow.Enrollments.CountByCourseIdsAsync(courseIds, ct).ConfigureAwait(false);

        return new TeacherDashboardViewModel
        {
            TotalCourses = courses.TotalCount,
            PublishedCourses = courses.Items.Count(c => c.IsPublished),
            TotalStudents = totalStudents,
            RecentCourses = mapper.Map<IReadOnlyList<CourseAdminListViewModel>>(
                courses.Items.Take(5).ToList())
        };
    }

    // ── 課程 ────────────────────────────────────────

    /// <inheritdoc />
    public async Task<PagedResult<CourseAdminListViewModel>> GetTeacherCoursesPagedAsync(
        string teacherId, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Courses.GetByTeacherIdPagedAsync(teacherId, page, pageSize, ct).ConfigureAwait(false);
        var items = mapper.Map<IReadOnlyList<CourseAdminListViewModel>>(result.Items);
        return new PagedResult<CourseAdminListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<CourseDetailAdminViewModel?> GetCourseDetailAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyCourseOwnershipAsync(id, teacherId, ct).ConfigureAwait(false))
            return null;
        return await adminService.GetCourseDetailAsync(id, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CourseFormViewModel?> GetCourseForEditAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyCourseOwnershipAsync(id, teacherId, ct).ConfigureAwait(false))
            return null;
        return await adminService.GetCourseForEditAsync(id, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateCourseAsync(
        CourseFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (await uow.Courses.ExistsByTitleAsync(vm.Title, ct).ConfigureAwait(false))
            return ServiceResult<int>.Failure("課程標題已存在");

        var course = mapper.Map<Data.Entities.Course>(vm);
        course.TeacherId = teacherId;
        await uow.Courses.AddAsync(course, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("教師建立課程 | TeacherId={TeacherId} | CourseId={CourseId} | Title={Title}",
            teacherId, course.Id, course.Title);
        return ServiceResult<int>.Success(course.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateCourseAsync(
        CourseFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyCourseOwnershipAsync(vm.Id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此課程");
        return await adminService.UpdateCourseAsync(vm, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteCourseAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyCourseOwnershipAsync(id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此課程");
        return await adminService.DeleteCourseAsync(id, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateCourseImageAsync(
        int courseId, string imageUrl, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyCourseOwnershipAsync(courseId, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此課程");
        return await adminService.UpdateCourseImageAsync(courseId, imageUrl, ct).ConfigureAwait(false);
    }

    // ── 章節 ───────────────────────────────────────

    /// <inheritdoc />
    public async Task<SectionFormViewModel?> GetSectionForEditAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifySectionOwnershipAsync(id, teacherId, ct).ConfigureAwait(false))
            return null;
        return await adminService.GetSectionForEditAsync(id, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateSectionAsync(
        SectionFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyCourseOwnershipAsync(vm.CourseId, teacherId, ct).ConfigureAwait(false))
            return ServiceResult<int>.Failure("無權限操作此課程");
        return await adminService.CreateSectionAsync(vm, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateSectionAsync(
        SectionFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifySectionOwnershipAsync(vm.Id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此章節");
        return await adminService.UpdateSectionAsync(vm, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteSectionAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifySectionOwnershipAsync(id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此章節");
        return await adminService.DeleteSectionAsync(id, ct).ConfigureAwait(false);
    }

    // ── 單元 ────────────────────────────────────────

    /// <inheritdoc />
    public async Task<LessonFormViewModel?> GetLessonForEditAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyLessonOwnershipAsync(id, teacherId, ct).ConfigureAwait(false))
            return null;
        return await adminService.GetLessonForEditAsync(id, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateLessonAsync(
        LessonFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifySectionOwnershipAsync(vm.SectionId, teacherId, ct).ConfigureAwait(false))
            return ServiceResult<int>.Failure("無權限操作此章節");
        return await adminService.CreateLessonAsync(vm, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateLessonAsync(
        LessonFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyLessonOwnershipAsync(vm.Id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此單元");
        return await adminService.UpdateLessonAsync(vm, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteLessonAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await VerifyLessonOwnershipAsync(id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此單元");
        return await adminService.DeleteLessonAsync(id, ct).ConfigureAwait(false);
    }

    // ── 所有權驗證 ──────────────────────────────────

    private async Task<bool> VerifyCourseOwnershipAsync(int courseId, string teacherId, CancellationToken ct)
        => await uow.Courses.IsOwnedByTeacherAsync(courseId, teacherId, ct).ConfigureAwait(false);

    private async Task<bool> VerifySectionOwnershipAsync(int sectionId, string teacherId, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(sectionId, ct).ConfigureAwait(false);
        if (section is null) return false;
        return await VerifyCourseOwnershipAsync(section.CourseId, teacherId, ct).ConfigureAwait(false);
    }

    private async Task<bool> VerifyLessonOwnershipAsync(int lessonId, string teacherId, CancellationToken ct)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null) return false;
        return await VerifySectionOwnershipAsync(lesson.SectionId, teacherId, ct).ConfigureAwait(false);
    }
}
