using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Infrastructure.Persistence;
using BCSMS.Infrastructure.Persistence.Repositories;
using BCSMS.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BCSMS.Infrastructure;

/// <summary>
/// Registers infrastructure services (database, repositories, time provider, etc.) into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<BcsmsDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Abstractions / Services
        services.AddSingleton<IClock, SystemClock>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

        return services;
    }
}
