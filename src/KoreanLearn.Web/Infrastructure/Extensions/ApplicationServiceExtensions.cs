using KoreanLearn.Service.Services.Implementation;
using KoreanLearn.Service.Services.Interfaces;

namespace KoreanLearn.Web.Infrastructure.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // AutoMapper - scan Service assembly for profiles
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(KoreanLearn.Service.Marker).Assembly);
        });

        // Services
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

        return services;
    }
}
