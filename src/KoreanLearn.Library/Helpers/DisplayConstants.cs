namespace KoreanLearn.Library.Helpers;

/// <summary>顯示相關常數（分頁筆數、前端版面限制）</summary>
public static class DisplayConstants
{
    /// <summary>前台課程列表每頁筆數</summary>
    public const int CoursePageSize = 12;

    /// <summary>後台管理列表每頁筆數</summary>
    public const int AdminPageSize = 20;

    /// <summary>教師管理列表每頁筆數</summary>
    public const int TeacherPageSize = 20;

    /// <summary>學生訂單列表每頁筆數</summary>
    public const int OrderPageSize = 10;

    /// <summary>討論區列表每頁筆數</summary>
    public const int DiscussionPageSize = 15;

    /// <summary>搜尋使用者列表每頁筆數</summary>
    public const int UserPageSize = 20;

    /// <summary>課程詳情「相關課程」最多顯示筆數</summary>
    public const int RelatedCoursesCount = 4;

    /// <summary>首頁精選課程最多顯示筆數</summary>
    public const int FeaturedCoursesCount = 8;

    /// <summary>通用「未知」預設文字</summary>
    public const string Unknown = "未知";
}
