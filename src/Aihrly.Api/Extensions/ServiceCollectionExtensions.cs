using Aihrly.Api.Data;
using Aihrly.Api.Services;
using Aihrly.Api.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Aihrly.Api.Infrastructure;

namespace Aihrly.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        // Scans the current assembly and registers all FluentValidation validators automatically
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IScoreService, ScoreService>();

        services.AddSingleton<INotificationQueue, NotificationQueue>();
        services.AddHostedService<NotificationWorker>();
        return services;
    }
}
