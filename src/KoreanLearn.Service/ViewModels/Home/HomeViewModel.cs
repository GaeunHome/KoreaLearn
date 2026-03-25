using KoreanLearn.Service.ViewModels.Course;

namespace KoreanLearn.Service.ViewModels.Home;

/// <summary>首頁 ViewModel（精選課程與公告）</summary>
public class HomeViewModel
{
    /// <summary>精選課程列表</summary>
    public IReadOnlyList<CourseListViewModel> FeaturedCourses { get; set; } = [];

    /// <summary>最新公告列表</summary>
    public IReadOnlyList<AnnouncementViewModel> Announcements { get; set; } = [];
}

/// <summary>公告 ViewModel</summary>
public class AnnouncementViewModel
{
    /// <summary>公告 ID</summary>
    public int Id { get; set; }

    /// <summary>公告標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>公告內容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }
}
