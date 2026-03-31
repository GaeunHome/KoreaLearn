using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>後台課程管理業務邏輯介面（Admin 專用，涵蓋課程、章節、單元的 CRUD）</summary>
public interface ICourseAdminService
{
    // ── 課程 ──

    /// <summary>取得後台課程分頁列表</summary>
    Task<PagedResult<CourseAdminListViewModel>> GetCoursesPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得課程詳情（含章節與單元）</summary>
    Task<CourseDetailAdminViewModel?> GetCourseDetailAsync(int id, CancellationToken ct = default);

    /// <summary>取得課程編輯表單資料</summary>
    Task<CourseFormViewModel?> GetCourseForEditAsync(int id, CancellationToken ct = default);

    /// <summary>建立新課程，回傳新課程 ID</summary>
    Task<ServiceResult<int>> CreateCourseAsync(CourseFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新課程基本資訊</summary>
    Task<ServiceResult> UpdateCourseAsync(CourseFormViewModel vm, CancellationToken ct = default);

    /// <summary>軟刪除課程</summary>
    Task<ServiceResult> DeleteCourseAsync(int id, CancellationToken ct = default);

    /// <summary>更新課程封面圖片網址</summary>
    Task<ServiceResult> UpdateCourseImageAsync(int courseId, string imageUrl, CancellationToken ct = default);

    // ── 章節 ──

    /// <summary>取得章節編輯表單資料</summary>
    Task<SectionFormViewModel?> GetSectionForEditAsync(int id, CancellationToken ct = default);

    /// <summary>建立新章節，回傳新章節 ID</summary>
    Task<ServiceResult<int>> CreateSectionAsync(SectionFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新章節資訊</summary>
    Task<ServiceResult> UpdateSectionAsync(SectionFormViewModel vm, CancellationToken ct = default);

    /// <summary>軟刪除章節</summary>
    Task<ServiceResult> DeleteSectionAsync(int id, CancellationToken ct = default);

    // ── 單元 ──

    /// <summary>取得單元編輯表單資料</summary>
    Task<LessonFormViewModel?> GetLessonForEditAsync(int id, CancellationToken ct = default);

    /// <summary>建立新單元，回傳新單元 ID</summary>
    Task<ServiceResult<int>> CreateLessonAsync(LessonFormViewModel vm, CancellationToken ct = default);

    /// <summary>更新單元資訊</summary>
    Task<ServiceResult> UpdateLessonAsync(LessonFormViewModel vm, CancellationToken ct = default);

    /// <summary>軟刪除單元</summary>
    Task<ServiceResult> DeleteLessonAsync(int id, CancellationToken ct = default);
}
