using BCSMS.Application.Auth;
using BCSMS.Application.ServiceRequests;
using Microsoft.Extensions.DependencyInjection;

namespace BCSMS.Application;

/// <summary>
/// Registers application layer services into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IServiceRequestService, ServiceRequestService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
