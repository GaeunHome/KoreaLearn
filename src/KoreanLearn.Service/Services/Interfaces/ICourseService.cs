using KoreanLearn.Library.Helpers;
using KoreanLearn.Service.ViewModels.Course;
using KoreanLearn.Service.ViewModels.Home;

namespace KoreanLearn.Service.Services.Interfaces;

/// <summary>前台課程瀏覽相關業務邏輯介面（公開，無需登入）</summary>
public interface ICourseService
{
    /// <summary>取得所有已發佈的課程列表</summary>
    Task<IReadOnlyList<CourseListViewModel>> GetPublishedCoursesAsync(CancellationToken ct = default);

    /// <summary>依關鍵字搜尋已發佈課程（分頁）</summary>
    Task<PagedResult<CourseListViewModel>> SearchCoursesAsync(string? keyword, int page, int pageSize, CancellationToken ct = default);

    /// <summary>取得單一課程詳情（含章節與單元），若使用者已登入則附帶選課狀態與學習進度</summary>
    Task<CourseDetailViewModel?> GetCourseDetailAsync(int id, string? userId = null, CancellationToken ct = default);

    /// <summary>取得首頁 ViewModel（精選課程、公告等）</summary>
    Task<HomeViewModel> GetHomeViewModelAsync(CancellationToken ct = default);
}
