using Aihrly.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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

    // We'll add AddApplicationServices() here as we build each feature
}
