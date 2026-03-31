using KoreanLearn.Library.Enums;
using Microsoft.AspNetCore.Identity;

namespace KoreanLearn.Data.Entities;

/// <summary>應用程式使用者實體，繼承 IdentityUser 並擴充學習相關欄位</summary>
public class AppUser : IdentityUser
{
    /// <summary>顯示名稱</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>頭像圖片路徑</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>偏好的兩步驟驗證方式</summary>
    public TwoFactorMethod PreferredTwoFactorMethod { get; set; } = TwoFactorMethod.None;

    /// <summary>帳號建立時間</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>帳號最後更新時間</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>連續學習天數</summary>
    public int ConsecutiveLearningDays { get; set; }

    /// <summary>最後一次學習日期</summary>
    public DateTime? LastLearningDate { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Progress> Progresses { get; set; } = [];
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = [];
    public ICollection<FlashcardLog> FlashcardLogs { get; set; } = [];
    public ICollection<Discussion> Discussions { get; set; } = [];
    public ICollection<DiscussionReply> DiscussionReplies { get; set; } = [];
    public ICollection<Course> TeacherCourses { get; set; } = [];
    public UserSubscription? ActiveSubscription { get; set; }
}
