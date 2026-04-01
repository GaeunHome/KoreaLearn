using KoreanLearn.Data.Repositories.Interfaces;

namespace KoreanLearn.Data.UnitOfWork;

/// <summary>Unit of Work 介面，統一管理所有 Repository 並提供交易控制</summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>課程 Repository</summary>
    ICourseRepository Courses { get; }

    /// <summary>章節 Repository</summary>
    ISectionRepository Sections { get; }

    /// <summary>單元 Repository</summary>
    ILessonRepository Lessons { get; }

    /// <summary>選課紀錄 Repository</summary>
    IEnrollmentRepository Enrollments { get; }

    /// <summary>訂單 Repository</summary>
    IOrderRepository Orders { get; }

    /// <summary>學習進度 Repository</summary>
    IProgressRepository Progresses { get; }

    /// <summary>公告 Repository</summary>
    IAnnouncementRepository Announcements { get; }

    /// <summary>公告附件 Repository</summary>
    IAnnouncementAttachmentRepository AnnouncementAttachments { get; }

    /// <summary>測驗 Repository</summary>
    IQuizRepository Quizzes { get; }

    /// <summary>測驗作答紀錄 Repository</summary>
    IQuizAttemptRepository QuizAttempts { get; }

    /// <summary>字卡牌組 Repository</summary>
    IFlashcardDeckRepository FlashcardDecks { get; }

    /// <summary>字卡學習紀錄 Repository</summary>
    IFlashcardLogRepository FlashcardLogs { get; }

    /// <summary>發音練習 Repository</summary>
    IPronunciationRepository Pronunciations { get; }

    /// <summary>訂閱方案 Repository</summary>
    ISubscriptionPlanRepository SubscriptionPlans { get; }

    /// <summary>使用者訂閱 Repository</summary>
    IUserSubscriptionRepository UserSubscriptions { get; }

    /// <summary>討論區 Repository</summary>
    IDiscussionRepository Discussions { get; }

    /// <summary>單元附件 Repository</summary>
    ILessonAttachmentRepository LessonAttachments { get; }

    /// <summary>系統參數 Repository</summary>
    ISystemSettingRepository SystemSettings { get; }

    /// <summary>幻燈片橫幅 Repository</summary>
    IBannerRepository Banners { get; }

    /// <summary>密碼歷史記錄 Repository</summary>
    IPasswordHistoryRepository PasswordHistories { get; }

    /// <summary>儲存所有變更至資料庫</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>開始資料庫交易</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>提交交易</summary>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>回滾交易</summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
