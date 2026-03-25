using KoreanLearn.Service.Services.Implementation;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Infrastructure.Extensions;

/// <summary>應用程式服務註冊擴充方法，註冊 AutoMapper 與所有 Service 層的 DI 對應</summary>
public static class ApplicationServiceExtensions
{
    /// <summary>註冊 AutoMapper Profile 與所有商業邏輯 Service（Scoped 生命週期）</summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // AutoMapper — 掃描 Service 組件中的所有 Profile
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(KoreanLearn.Service.Marker).Assembly);
        });

        // ── 商業邏輯 Services ──
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ICourseAdminService, CourseAdminService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<ILessonPlayerService, LessonPlayerService>();
        services.AddScoped<IQuizAdminService, QuizAdminService>();
        services.AddScoped<IQuizTakeService, QuizTakeService>();
        services.AddScoped<IFlashcardAdminService, FlashcardAdminService>();
        services.AddScoped<IFlashcardLearnService, FlashcardLearnService>();
        services.AddScoped<IPronunciationService, PronunciationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDiscussionService, DiscussionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<ITeacherCourseService, TeacherCourseService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
