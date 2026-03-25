using KoreanLearn.Service.ViewModels.Admin.Course;

namespace KoreanLearn.Service.ViewModels.Teacher;

/// <summary>教師儀表板 ViewModel（課程統計與近期課程）</summary>
public class TeacherDashboardViewModel
{
    /// <summary>該教師的課程總數</summary>
    public int TotalCourses { get; set; }

    /// <summary>已發佈的課程數</summary>
    public int PublishedCourses { get; set; }

    /// <summary>所有課程的學生總數</summary>
    public int TotalStudents { get; set; }

    /// <summary>近期課程列表（最多 5 筆）</summary>
    public IReadOnlyList<CourseAdminListViewModel> RecentCourses { get; set; } = [];
}
