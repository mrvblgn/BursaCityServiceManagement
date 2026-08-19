using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Abstractions.Security;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Infrastructure.Persistence;
using BCSMS.Infrastructure.Persistence.Repositories;
using BCSMS.Infrastructure.Persistence.Seeding;
using BCSMS.Infrastructure.Security;
using BCSMS.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BCSMS.Infrastructure;

/// <summary>
/// Registers infrastructure services (database, repositories, time provider, security, seeding, etc.) into the DI container.
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

        // Options
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Time & Security Services
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

        // Seeding
        services.AddScoped<IDbSeeder, DbSeeder>();

        return services;
    }
}
