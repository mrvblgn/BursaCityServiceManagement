using Microsoft.Extensions.DependencyInjection;

namespace BCSMS.Infrastructure;

/// <summary>
/// Registers infrastructure services (database, external APIs, etc.) into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Infrastructure services (DbContext, repositories, external clients, etc.)
        // will be registered here in future phases.
        return services;
    }
}
