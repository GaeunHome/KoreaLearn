using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace KoreanLearn.Data.UnitOfWork;

/// <summary>Unit of Work 實作，透過 primary constructor 注入所有 Repository</summary>
public class UnitOfWork(
    ApplicationDbContext db,
    ICourseRepository courses,
    ISectionRepository sections,
    ILessonRepository lessons,
    IEnrollmentRepository enrollments,
    IOrderRepository orders,
    IProgressRepository progresses,
    IAnnouncementRepository announcements,
    IQuizRepository quizzes,
    IQuizAttemptRepository quizAttempts,
    IFlashcardDeckRepository flashcardDecks,
    IFlashcardLogRepository flashcardLogs,
    IPronunciationRepository pronunciations,
    ISubscriptionPlanRepository subscriptionPlans,
    IUserSubscriptionRepository userSubscriptions,
    IDiscussionRepository discussions,
    ILessonAttachmentRepository lessonAttachments) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    // ── Repository 屬性 ──────────────────────────────────
    public ICourseRepository Courses => courses;
    public ISectionRepository Sections => sections;
    public ILessonRepository Lessons => lessons;
    public IEnrollmentRepository Enrollments => enrollments;
    public IOrderRepository Orders => orders;
    public IProgressRepository Progresses => progresses;
    public IAnnouncementRepository Announcements => announcements;
    public IQuizRepository Quizzes => quizzes;
    public IQuizAttemptRepository QuizAttempts => quizAttempts;
    public IFlashcardDeckRepository FlashcardDecks => flashcardDecks;
    public IFlashcardLogRepository FlashcardLogs => flashcardLogs;
    public IPronunciationRepository Pronunciations => pronunciations;
    public ISubscriptionPlanRepository SubscriptionPlans => subscriptionPlans;
    public IUserSubscriptionRepository UserSubscriptions => userSubscriptions;
    public IDiscussionRepository Discussions => discussions;
    public ILessonAttachmentRepository LessonAttachments => lessonAttachments;

    // ── 持久化與交易控制 ─────────────────────────────────
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        db.Dispose();
    }
}
