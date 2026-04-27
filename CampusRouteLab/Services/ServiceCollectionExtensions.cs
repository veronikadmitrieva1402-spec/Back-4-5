namespace CampusRouteLab.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCampusServices(this IServiceCollection services)
    {
        services.AddSingleton<IStudentCatalogService, StudentCatalogService>();
        services.AddSingleton<IAppInfoService, AppInfoService>();
        
        services.AddScoped<IRequestContextService, RequestContextService>();
        
        services.AddTransient<ITransientMarkerService, TransientMarkerService>();
        services.AddTransient<DiagnosticsReportService>();
        
        return services;
    }
}