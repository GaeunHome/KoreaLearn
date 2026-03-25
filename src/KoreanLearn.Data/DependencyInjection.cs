using KoreanLearn.Data.Repositories.Implementation;
using KoreanLearn.Data.Repositories.Interfaces;
using KoreanLearn.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KoreanLearn.Data;

/// <summary>資料層 DI 擴充方法，註冊 DbContext、Repository 與 UnitOfWork</summary>
public static class DependencyInjection
{
    /// <summary>將資料層所有服務註冊至 DI 容器</summary>
    public static IServiceCollection AddDataLayer(
        this IServiceCollection services, string connectionString)
    {
        // ── DbContext ────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(connectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        // DbContextFactory（平行查詢、BackgroundService 用）
        services.AddDbContextFactory<ApplicationDbContext>(opts =>
            opts.UseSqlServer(connectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)),
            lifetime: ServiceLifetime.Scoped);

        // ── Repositories ─────────────────────────────────
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IFlashcardDeckRepository, FlashcardDeckRepository>();
        services.AddScoped<IFlashcardLogRepository, FlashcardLogRepository>();
        services.AddScoped<IPronunciationRepository, PronunciationRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
        services.AddScoped<IDiscussionRepository, DiscussionRepository>();
        services.AddScoped<ILessonAttachmentRepository, LessonAttachmentRepository>();

        // ── UnitOfWork ───────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}
