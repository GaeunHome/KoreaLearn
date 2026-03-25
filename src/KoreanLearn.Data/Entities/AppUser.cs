using Microsoft.AspNetCore.Identity;

namespace KoreanLearn.Data.Entities;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ConsecutiveLearningDays { get; set; }
    public DateTime? LastLearningDate { get; set; }

    // Navigation
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
