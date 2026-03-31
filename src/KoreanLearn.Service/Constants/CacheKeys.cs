namespace KoreanLearn.Service.Constants;

/// <summary>快取鍵值常數，採階層命名便於前綴批次清除</summary>
public static class CacheKeys
{
    /// <summary>已發布的課程列表</summary>
    public const string PublishedCourses = "courses:published";

    /// <summary>課程相關快取前綴</summary>
    public const string CoursesPrefix = "courses:";

    /// <summary>公告列表</summary>
    public const string Announcements = "announcements:active";

    /// <summary>公告相關快取前綴</summary>
    public const string AnnouncementsPrefix = "announcements:";

    /// <summary>訂閱方案列表</summary>
    public const string SubscriptionPlans = "subscriptions:plans";

    /// <summary>訂閱相關快取前綴</summary>
    public const string SubscriptionsPrefix = "subscriptions:";

    /// <summary>字卡牌組列表</summary>
    public const string FlashcardDecks = "flashcards:decks";

    /// <summary>字卡相關快取前綴</summary>
    public const string FlashcardsPrefix = "flashcards:";
}
