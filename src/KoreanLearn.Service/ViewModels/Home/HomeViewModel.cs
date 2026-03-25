using KoreanLearn.Service.ViewModels.Course;

namespace KoreanLearn.Service.ViewModels.Home;

public class HomeViewModel
{
    public IReadOnlyList<CourseListViewModel> FeaturedCourses { get; set; } = [];
    public IReadOnlyList<AnnouncementViewModel> Announcements { get; set; } = [];
}

public class AnnouncementViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
