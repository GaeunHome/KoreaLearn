using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;
using KoreanLearn.Service.ViewModels.Teacher;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>教師課程管理業務邏輯介面（僅限操作自己擁有的課程，所有操作皆驗證所有權）</summary>
public interface ITeacherCourseService
{
    /// <summary>取得教師儀表板資料（課程數、學生數、近期課程）</summary>
    Task<TeacherDashboardViewModel> GetDashboardAsync(string teacherId, CancellationToken ct = default);

    /// <summary>取得教師自己的課程分頁列表</summary>
    Task<PagedResult<CourseAdminListViewModel>> GetTeacherCoursesPagedAsync(string teacherId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得教師自己的課程詳情（含章節與單元）</summary>
    Task<CourseDetailAdminViewModel?> GetCourseDetailAsync(int id, string teacherId, CancellationToken ct = default);

    /// <summary>取得課程編輯表單資料（驗證所有權）</summary>
    Task<CourseFormViewModel?> GetCourseForEditAsync(int id, string teacherId, CancellationToken ct = default);

    /// <summary>教師建立新課程，自動設定 TeacherId</summary>
    Task<ServiceResult<int>> CreateCourseAsync(CourseFormViewModel vm, string teacherId, CancellationToken ct = default);

    /// <summary>更新教師自己的課程資訊</summary>
    Task<ServiceResult> UpdateCourseAsync(CourseFormViewModel vm, string teacherId, CancellationToken ct = default);

    /// <summary>軟刪除教師自己的課程</summary>
    Task<ServiceResult> DeleteCourseAsync(int id, string teacherId, CancellationToken ct = default);

    /// <summary>更新教師自己的課程封面圖片</summary>
    Task<ServiceResult> UpdateCourseImageAsync(int courseId, string imageUrl, string teacherId, CancellationToken ct = default);

    // ── 章節 — 透過課程所有權驗證 ──

    /// <summary>取得章節編輯表單資料（驗證課程所有權）</summary>
    Task<SectionFormViewModel?> GetSectionForEditAsync(int id, string teacherId, CancellationToken ct = default);

    /// <summary>建立新章節（驗證課程所有權）</summary>
    Task<ServiceResult<int>> CreateSectionAsync(SectionFormViewModel vm, string teacherId, CancellationToken ct = default);

    /// <summary>更新章節（驗證課程所有權）</summary>
    Task<ServiceResult> UpdateSectionAsync(SectionFormViewModel vm, string teacherId, CancellationToken ct = default);

    /// <summary>軟刪除章節（驗證課程所有權）</summary>
    Task<ServiceResult> DeleteSectionAsync(int id, string teacherId, CancellationToken ct = default);

    // ── 單元 — 透過課程所有權驗證 ──

    /// <summary>取得單元編輯表單資料（驗證課程所有權）</summary>
    Task<LessonFormViewModel?> GetLessonForEditAsync(int id, string teacherId, CancellationToken ct = default);

    /// <summary>建立新單元（驗證課程所有權）</summary>
    Task<ServiceResult<int>> CreateLessonAsync(LessonFormViewModel vm, string teacherId, CancellationToken ct = default);

    /// <summary>更新單元（驗證課程所有權）</summary>
    Task<ServiceResult> UpdateLessonAsync(LessonFormViewModel vm, string teacherId, CancellationToken ct = default);

    /// <summary>軟刪除單元（驗證課程所有權）</summary>
    Task<ServiceResult> DeleteLessonAsync(int id, string teacherId, CancellationToken ct = default);
}
