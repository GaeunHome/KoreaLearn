using KoreanLearn.Data.Repositories.Implementation;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KoreanLearn.Data.UnitOfWork;

/// <summary>
/// Unit of Work 實作，透過 IDbContextFactory 建立 DbContext 實例。
/// Repository 延遲載入（??=），所有 Repository 共用同一個 DbContext，
/// SaveChangesAsync 一次提交所有變更。
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // ── Repository 延遲載入欄位 ────────────────────────────
    private ICourseRepository? _courses;
    private ISectionRepository? _sections;
    private ILessonRepository? _lessons;
    private IEnrollmentRepository? _enrollments;
    private IOrderRepository? _orders;
    private IProgressRepository? _progresses;
    private IAnnouncementRepository? _announcements;
    private IQuizRepository? _quizzes;
    private IQuizAttemptRepository? _quizAttempts;
    private IFlashcardDeckRepository? _flashcardDecks;
    private IFlashcardLogRepository? _flashcardLogs;
    private IPronunciationRepository? _pronunciations;
    private ISubscriptionPlanRepository? _subscriptionPlans;
    private IUserSubscriptionRepository? _userSubscriptions;
    private IDiscussionRepository? _discussions;
    private ILessonAttachmentRepository? _lessonAttachments;
    private ISystemSettingRepository? _systemSettings;
    private IBannerRepository? _banners;
    private IPasswordHistoryRepository? _passwordHistories;

    public UnitOfWork(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _context = contextFactory.CreateDbContext();
    }

    // ── Repository 屬性（延遲初始化）──────────────────────
    /// <summary>課程 Repository</summary>
    public ICourseRepository Courses => _courses ??= new CourseRepository(_context);

    /// <summary>章節 Repository</summary>
    public ISectionRepository Sections => _sections ??= new SectionRepository(_context);

    /// <summary>單元 Repository</summary>
    public ILessonRepository Lessons => _lessons ??= new LessonRepository(_context);

    /// <summary>選課紀錄 Repository</summary>
    public IEnrollmentRepository Enrollments => _enrollments ??= new EnrollmentRepository(_context);

    /// <summary>訂單 Repository</summary>
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    /// <summary>學習進度 Repository</summary>
    public IProgressRepository Progresses => _progresses ??= new ProgressRepository(_context);

    /// <summary>公告 Repository</summary>
    public IAnnouncementRepository Announcements => _announcements ??= new AnnouncementRepository(_context);

    /// <summary>測驗 Repository</summary>
    public IQuizRepository Quizzes => _quizzes ??= new QuizRepository(_context);

    /// <summary>測驗作答紀錄 Repository</summary>
    public IQuizAttemptRepository QuizAttempts => _quizAttempts ??= new QuizAttemptRepository(_context);

    /// <summary>字卡牌組 Repository</summary>
    public IFlashcardDeckRepository FlashcardDecks => _flashcardDecks ??= new FlashcardDeckRepository(_context);

    /// <summary>字卡學習紀錄 Repository</summary>
    public IFlashcardLogRepository FlashcardLogs => _flashcardLogs ??= new FlashcardLogRepository(_context);

    /// <summary>發音練習 Repository</summary>
    public IPronunciationRepository Pronunciations => _pronunciations ??= new PronunciationRepository(_context);

    /// <summary>訂閱方案 Repository</summary>
    public ISubscriptionPlanRepository SubscriptionPlans => _subscriptionPlans ??= new SubscriptionPlanRepository(_context);

    /// <summary>使用者訂閱 Repository</summary>
    public IUserSubscriptionRepository UserSubscriptions => _userSubscriptions ??= new UserSubscriptionRepository(_context);

    /// <summary>討論區 Repository</summary>
    public IDiscussionRepository Discussions => _discussions ??= new DiscussionRepository(_context);

    /// <summary>單元附件 Repository</summary>
    public ILessonAttachmentRepository LessonAttachments => _lessonAttachments ??= new LessonAttachmentRepository(_context);

    /// <summary>系統參數 Repository</summary>
    public ISystemSettingRepository SystemSettings => _systemSettings ??= new SystemSettingRepository(_context);

    /// <summary>幻燈片橫幅 Repository</summary>
    public IBannerRepository Banners => _banners ??= new BannerRepository(_context);

    /// <summary>密碼歷史記錄 Repository</summary>
    public IPasswordHistoryRepository PasswordHistories => _passwordHistories ??= new PasswordHistoryRepository(_context);

    // ── 持久化與交易控制 ──────────────────────────────────

    /// <summary>儲存所有變更至資料庫</summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct).ConfigureAwait(false);

    /// <summary>開始資料庫交易</summary>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("已有進行中的交易，請先 Commit 或 Rollback。");
        _transaction = await _context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
    }

    /// <summary>提交交易</summary>
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await SaveChangesAsync(ct).ConfigureAwait(false);
            if (_transaction is not null)
            {
                await _transaction.CommitAsync(ct).ConfigureAwait(false);
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _transaction = null;
            }
        }
        catch
        {
            await RollbackTransactionAsync(ct).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>回滾交易</summary>
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    /// <summary>釋放資源</summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
