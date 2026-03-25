using AutoMapper;
using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;
using KoreanLearn.Service.ViewModels.Teacher;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

public class TeacherCourseService(
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<TeacherCourseService> logger) : ITeacherCourseService
{
    // ── Dashboard ─────────────────────────────────────

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

    // ── Course ────────────────────────────────────────

    public async Task<PagedResult<CourseAdminListViewModel>> GetTeacherCoursesPagedAsync(
        string teacherId, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await uow.Courses.GetByTeacherIdPagedAsync(teacherId, page, pageSize, ct).ConfigureAwait(false);
        var items = mapper.Map<IReadOnlyList<CourseAdminListViewModel>>(result.Items);
        return new PagedResult<CourseAdminListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task<CourseDetailAdminViewModel?> GetCourseDetailAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await uow.Courses.IsOwnedByTeacherAsync(id, teacherId, ct).ConfigureAwait(false))
            return null;
        var course = await uow.Courses.GetWithSectionsAndLessonsAsync(id, ct).ConfigureAwait(false);
        return course is null ? null : mapper.Map<CourseDetailAdminViewModel>(course);
    }

    public async Task<EditCourseViewModel?> GetCourseForEditAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await uow.Courses.IsOwnedByTeacherAsync(id, teacherId, ct).ConfigureAwait(false))
            return null;
        var course = await uow.Courses.GetByIdAsync(id, ct).ConfigureAwait(false);
        return course is null ? null : mapper.Map<EditCourseViewModel>(course);
    }

    public async Task<ServiceResult<int>> CreateCourseAsync(
        CreateCourseViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (await uow.Courses.ExistsByTitleAsync(vm.Title, ct).ConfigureAwait(false))
            return ServiceResult<int>.Failure("課程標題已存在");

        var course = mapper.Map<Course>(vm);
        course.TeacherId = teacherId;
        await uow.Courses.AddAsync(course, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("教師建立課程 | TeacherId={TeacherId} | CourseId={CourseId} | Title={Title}",
            teacherId, course.Id, course.Title);
        return ServiceResult<int>.Success(course.Id);
    }

    public async Task<ServiceResult> UpdateCourseAsync(
        EditCourseViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await uow.Courses.IsOwnedByTeacherAsync(vm.Id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此課程");

        var course = await uow.Courses.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (course is null) return ServiceResult.Failure("課程不存在");

        course.Title = vm.Title;
        course.Description = vm.Description;
        course.Price = vm.Price;
        course.Level = vm.Level;
        course.IsPublished = vm.IsPublished;
        course.SortOrder = vm.SortOrder;

        uow.Courses.Update(course);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCourseAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await uow.Courses.IsOwnedByTeacherAsync(id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此課程");

        var course = await uow.Courses.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (course is null) return ServiceResult.Failure("課程不存在");

        uow.Courses.Remove(course);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateCourseImageAsync(
        int courseId, string imageUrl, string teacherId, CancellationToken ct = default)
    {
        if (!await uow.Courses.IsOwnedByTeacherAsync(courseId, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此課程");

        var course = await uow.Courses.GetByIdAsync(courseId, ct).ConfigureAwait(false);
        if (course is null) return ServiceResult.Failure("課程不存在");

        course.CoverImageUrl = imageUrl;
        uow.Courses.Update(course);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    // ── Section ───────────────────────────────────────

    private async Task<bool> IsSectionOwnedAsync(int sectionId, string teacherId, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(sectionId, ct).ConfigureAwait(false);
        if (section is null) return false;
        return await uow.Courses.IsOwnedByTeacherAsync(section.CourseId, teacherId, ct).ConfigureAwait(false);
    }

    private async Task<bool> IsLessonOwnedAsync(int lessonId, string teacherId, CancellationToken ct)
    {
        var lesson = await uow.Lessons.GetByIdAsync(lessonId, ct).ConfigureAwait(false);
        if (lesson is null) return false;
        return await IsSectionOwnedAsync(lesson.SectionId, teacherId, ct).ConfigureAwait(false);
    }

    public async Task<SectionFormViewModel?> GetSectionForEditAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await IsSectionOwnedAsync(id, teacherId, ct).ConfigureAwait(false)) return null;
        var section = await uow.Sections.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (section is null) return null;
        var course = await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false);
        var vm = mapper.Map<SectionFormViewModel>(section);
        vm.CourseTitle = course?.Title;
        return vm;
    }

    public async Task<ServiceResult<int>> CreateSectionAsync(
        SectionFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await uow.Courses.IsOwnedByTeacherAsync(vm.CourseId, teacherId, ct).ConfigureAwait(false))
            return ServiceResult<int>.Failure("無權限操作此課程");

        var section = mapper.Map<Section>(vm);
        await uow.Sections.AddAsync(section, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult<int>.Success(section.Id);
    }

    public async Task<ServiceResult> UpdateSectionAsync(
        SectionFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await IsSectionOwnedAsync(vm.Id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此章節");

        var section = await uow.Sections.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (section is null) return ServiceResult.Failure("章節不存在");

        section.Title = vm.Title;
        section.Description = vm.Description;
        section.SortOrder = vm.SortOrder;
        uow.Sections.Update(section);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteSectionAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await IsSectionOwnedAsync(id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此章節");

        var section = await uow.Sections.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (section is null) return ServiceResult.Failure("章節不存在");

        uow.Sections.Remove(section);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    // ── Lesson ────────────────────────────────────────

    public async Task<LessonFormViewModel?> GetLessonForEditAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await IsLessonOwnedAsync(id, teacherId, ct).ConfigureAwait(false)) return null;
        var lesson = await uow.Lessons.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (lesson is null) return null;

        var section = await uow.Sections.GetByIdAsync(lesson.SectionId, ct).ConfigureAwait(false);
        var course = section is not null
            ? await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false)
            : null;

        var vm = mapper.Map<LessonFormViewModel>(lesson);
        vm.SectionTitle = section?.Title;
        vm.CourseTitle = course?.Title;
        vm.CourseId = course?.Id;
        vm.ExistingVideoUrl = lesson.VideoUrl;
        vm.ExistingPdfUrl = lesson.PdfUrl;
        vm.ExistingPdfFileName = lesson.PdfFileName;
        return vm;
    }

    public async Task<ServiceResult<int>> CreateLessonAsync(
        LessonFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await IsSectionOwnedAsync(vm.SectionId, teacherId, ct).ConfigureAwait(false))
            return ServiceResult<int>.Failure("無權限操作此章節");

        var lesson = mapper.Map<Lesson>(vm);
        lesson.VideoUrl = vm.ExistingVideoUrl;
        lesson.PdfUrl = vm.ExistingPdfUrl;
        lesson.PdfFileName = vm.ExistingPdfFileName;

        await uow.Lessons.AddAsync(lesson, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult<int>.Success(lesson.Id);
    }

    public async Task<ServiceResult> UpdateLessonAsync(
        LessonFormViewModel vm, string teacherId, CancellationToken ct = default)
    {
        if (!await IsLessonOwnedAsync(vm.Id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此單元");

        var lesson = await uow.Lessons.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (lesson is null) return ServiceResult.Failure("單元不存在");

        lesson.Title = vm.Title;
        lesson.Description = vm.Description;
        lesson.Type = vm.Type;
        lesson.SortOrder = vm.SortOrder;
        lesson.IsFreePreview = vm.IsFreePreview;
        lesson.ArticleContent = vm.ArticleContent;
        lesson.VideoDurationSeconds = vm.VideoDurationSeconds;

        if (vm.ExistingVideoUrl is not null) lesson.VideoUrl = vm.ExistingVideoUrl;
        if (vm.ExistingPdfUrl is not null)
        {
            lesson.PdfUrl = vm.ExistingPdfUrl;
            lesson.PdfFileName = vm.ExistingPdfFileName;
        }

        uow.Lessons.Update(lesson);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteLessonAsync(
        int id, string teacherId, CancellationToken ct = default)
    {
        if (!await IsLessonOwnedAsync(id, teacherId, ct).ConfigureAwait(false))
            return ServiceResult.Failure("無權限操作此單元");

        var lesson = await uow.Lessons.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (lesson is null) return ServiceResult.Failure("單元不存在");

        uow.Lessons.Remove(lesson);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return ServiceResult.Success();
    }
}
