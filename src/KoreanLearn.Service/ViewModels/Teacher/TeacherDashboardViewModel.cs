using KoreanLearn.Service.ViewModels.Admin.Course;

namespace KoreanLearn.Service.ViewModels.Teacher;

public class TeacherDashboardViewModel
{
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int TotalStudents { get; set; }
    public IReadOnlyList<CourseAdminListViewModel> RecentCourses { get; set; } = [];
}
