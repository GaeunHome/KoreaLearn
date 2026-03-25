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

        // Services will be registered here as they are created
        // services.AddScoped<ICourseService, CourseService>();

        return services;
    }
}
