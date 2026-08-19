using Microsoft.Extensions.DependencyInjection;

namespace BCSMS.Application;

/// <summary>
/// Registers application layer services into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services (MediatR, validators, etc.) will be registered here in future phases.
        return services;
    }
}
