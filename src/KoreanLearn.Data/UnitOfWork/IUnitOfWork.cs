using KoreanLearn.Data.Repositories.Interfaces;

namespace KoreanLearn.Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    ICourseRepository Courses { get; }
    ISectionRepository Sections { get; }
    ILessonRepository Lessons { get; }
    IEnrollmentRepository Enrollments { get; }
    IOrderRepository Orders { get; }
    IProgressRepository Progresses { get; }
    IAnnouncementRepository Announcements { get; }
    IQuizRepository Quizzes { get; }
    IQuizAttemptRepository QuizAttempts { get; }
    IFlashcardDeckRepository FlashcardDecks { get; }
    IFlashcardLogRepository FlashcardLogs { get; }
    IPronunciationRepository Pronunciations { get; }
    ISubscriptionPlanRepository SubscriptionPlans { get; }
    IUserSubscriptionRepository UserSubscriptions { get; }
    IDiscussionRepository Discussions { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
