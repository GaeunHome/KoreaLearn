using MapsterMapper;
using KoreanLearn.Data.Entities;
using KoreanLearn.Data.UnitOfWork;
using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.Services.Interfaces;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;
using Microsoft.Extensions.Logging;

namespace KoreanLearn.Service.Services.Implementation;

/// <summary>後台課程管理業務邏輯實作，處理課程、章節、單元的 CRUD（Admin 專用）</summary>
public class CourseAdminService(
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<CourseAdminService> logger) : ICourseAdminService
{
    // ── 課程 ──────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<PagedResult<CourseAdminListViewModel>> GetCoursesPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        logger.LogInformation("後台查詢課程列表 | Page={Page} | PageSize={PageSize}", page, pageSize);
        var result = await uow.Courses.GetPagedAsync(page, pageSize, ct).ConfigureAwait(false);
        var items = mapper.Map<IReadOnlyList<CourseAdminListViewModel>>(result.Items);
        return new PagedResult<CourseAdminListViewModel>(items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <inheritdoc />
    public async Task<CourseDetailAdminViewModel?> GetCourseDetailAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("後台查詢課程詳情 | CourseId={CourseId}", id);
        var course = await uow.Courses.GetWithSectionsAndLessonsAsync(id, ct).ConfigureAwait(false);
        if (course is null)
        {
            logger.LogWarning("課程不存在 | CourseId={CourseId}", id);
            return null;
        }
        return mapper.Map<CourseDetailAdminViewModel>(course);
    }

    /// <inheritdoc />
    public async Task<CourseFormViewModel?> GetCourseForEditAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("取得課程編輯資料 | CourseId={CourseId}", id);
        var course = await uow.Courses.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (course is null)
        {
            logger.LogWarning("取得課程編輯資料失敗：課程不存在 | CourseId={CourseId}", id);
            return null;
        }
        return mapper.Map<CourseFormViewModel>(course);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateCourseAsync(
        CourseFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("建立課程 | Title={Title} | Price={Price}", vm.Title, vm.Price);

        if (await uow.Courses.ExistsByTitleAsync(vm.Title, ct).ConfigureAwait(false))
        {
            logger.LogWarning("課程標題重複 | Title={Title}", vm.Title);
            return ServiceResult<int>.Failure("課程標題已存在");
        }

        var course = mapper.Map<Course>(vm);
        await uow.Courses.AddAsync(course, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("課程建立成功 | CourseId={CourseId} | Title={Title}", course.Id, course.Title);
        return ServiceResult<int>.Success(course.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateCourseAsync(
        CourseFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("更新課程 | CourseId={CourseId} | Title={Title}", vm.Id, vm.Title);

        var course = await uow.Courses.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (course is null)
        {
            logger.LogWarning("更新失敗：課程不存在 | CourseId={CourseId}", vm.Id);
            return ServiceResult.Failure("課程不存在");
        }

        course.Title = vm.Title;
        course.Description = vm.Description;
        course.Price = vm.Price;
        course.Level = vm.Level;
        course.IsPublished = vm.IsPublished;
        course.SortOrder = vm.SortOrder;

        uow.Courses.Update(course);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("課程更新成功 | CourseId={CourseId}", course.Id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteCourseAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("刪除課程 | CourseId={CourseId}", id);

        var course = await uow.Courses.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (course is null)
        {
            logger.LogWarning("刪除失敗：課程不存在 | CourseId={CourseId}", id);
            return ServiceResult.Failure("課程不存在");
        }

        uow.Courses.Remove(course); // 軟刪除由 DbContext 攔截
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("課程刪除成功（軟刪除）| CourseId={CourseId} | Title={Title}", id, course.Title);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateCourseImageAsync(
        int courseId, string imageUrl, CancellationToken ct = default)
    {
        logger.LogInformation("更新課程封面圖片 | CourseId={CourseId} | ImageUrl={ImageUrl}", courseId, imageUrl);
        var course = await uow.Courses.GetByIdAsync(courseId, ct).ConfigureAwait(false);
        if (course is null)
        {
            logger.LogWarning("更新課程封面圖片失敗：課程不存在 | CourseId={CourseId}", courseId);
            return ServiceResult.Failure("課程不存在");
        }

        course.CoverImageUrl = imageUrl;
        uow.Courses.Update(course);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("課程封面圖片更新成功 | CourseId={CourseId}", courseId);
        return ServiceResult.Success();
    }

    // ── 章節 ─────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<SectionFormViewModel?> GetSectionForEditAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("取得章節編輯資料 | SectionId={SectionId}", id);
        var section = await uow.Sections.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (section is null)
        {
            logger.LogWarning("取得章節編輯資料失敗：章節不存在 | SectionId={SectionId}", id);
            return null;
        }

        var course = await uow.Courses.GetByIdAsync(section.CourseId, ct).ConfigureAwait(false);
        var vm = mapper.Map<SectionFormViewModel>(section);
        vm.CourseTitle = course?.Title;
        return vm;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateSectionAsync(
        SectionFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("建立章節 | CourseId={CourseId} | Title={Title}", vm.CourseId, vm.Title);

        var course = await uow.Courses.GetByIdAsync(vm.CourseId, ct).ConfigureAwait(false);
        if (course is null)
            return ServiceResult<int>.Failure("課程不存在");

        var section = mapper.Map<Section>(vm);
        await uow.Sections.AddAsync(section, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("章節建立成功 | SectionId={SectionId} | Title={Title}", section.Id, section.Title);
        return ServiceResult<int>.Success(section.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateSectionAsync(
        SectionFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("更新章節 | SectionId={SectionId} | Title={Title}", vm.Id, vm.Title);

        var section = await uow.Sections.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (section is null)
            return ServiceResult.Failure("章節不存在");

        section.Title = vm.Title;
        section.Description = vm.Description;
        section.SortOrder = vm.SortOrder;

        uow.Sections.Update(section);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("章節更新成功 | SectionId={SectionId}", section.Id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteSectionAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("刪除章節 | SectionId={SectionId}", id);

        var section = await uow.Sections.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (section is null)
            return ServiceResult.Failure("章節不存在");

        uow.Sections.Remove(section);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("章節刪除成功（軟刪除）| SectionId={SectionId} | Title={Title}", id, section.Title);
        return ServiceResult.Success();
    }

    // ── 單元 ──────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<LessonFormViewModel?> GetLessonForEditAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("取得單元編輯資料 | LessonId={LessonId}", id);
        var lesson = await uow.Lessons.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (lesson is null)
        {
            logger.LogWarning("取得單元編輯資料失敗：單元不存在 | LessonId={LessonId}", id);
            return null;
        }

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

    /// <inheritdoc />
    public async Task<ServiceResult<int>> CreateLessonAsync(
        LessonFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("建立單元 | SectionId={SectionId} | Title={Title} | Type={Type}",
            vm.SectionId, vm.Title, vm.Type);

        var section = await uow.Sections.GetByIdAsync(vm.SectionId, ct).ConfigureAwait(false);
        if (section is null)
            return ServiceResult<int>.Failure("章節不存在");

        var lesson = mapper.Map<Lesson>(vm);
        lesson.VideoUrl = vm.ExistingVideoUrl;
        lesson.PdfUrl = vm.ExistingPdfUrl;
        lesson.PdfFileName = vm.ExistingPdfFileName;

        await uow.Lessons.AddAsync(lesson, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("單元建立成功 | LessonId={LessonId} | Title={Title}", lesson.Id, lesson.Title);
        return ServiceResult<int>.Success(lesson.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateLessonAsync(
        LessonFormViewModel vm, CancellationToken ct = default)
    {
        logger.LogInformation("更新單元 | LessonId={LessonId} | Title={Title}", vm.Id, vm.Title);

        var lesson = await uow.Lessons.GetByIdAsync(vm.Id, ct).ConfigureAwait(false);
        if (lesson is null)
            return ServiceResult.Failure("單元不存在");

        lesson.Title = vm.Title;
        lesson.Description = vm.Description;
        lesson.Type = vm.Type;
        lesson.SortOrder = vm.SortOrder;
        lesson.IsFreePreview = vm.IsFreePreview;
        lesson.ArticleContent = vm.ArticleContent;
        lesson.VideoDurationSeconds = vm.VideoDurationSeconds;

        if (vm.ExistingVideoUrl is not null)
            lesson.VideoUrl = vm.ExistingVideoUrl;
        if (vm.ExistingPdfUrl is not null)
        {
            lesson.PdfUrl = vm.ExistingPdfUrl;
            lesson.PdfFileName = vm.ExistingPdfFileName;
        }

        uow.Lessons.Update(lesson);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("單元更新成功 | LessonId={LessonId}", lesson.Id);
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteLessonAsync(
        int id, CancellationToken ct = default)
    {
        logger.LogInformation("刪除單元 | LessonId={LessonId}", id);

        var lesson = await uow.Lessons.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (lesson is null)
            return ServiceResult.Failure("單元不存在");

        uow.Lessons.Remove(lesson);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("單元刪除成功（軟刪除）| LessonId={LessonId} | Title={Title}", id, lesson.Title);
        return ServiceResult.Success();
    }
}
