using BCSMS.Application.Auth;
using BCSMS.Application.ServiceRequests;
using BCSMS.Application.ServiceRequests.Employee;
using BCSMS.Application.ServiceRequests.Manager;
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
        services.AddScoped<IManagerServiceRequestService, ManagerServiceRequestService>();
        services.AddScoped<IEmployeeServiceRequestService, EmployeeServiceRequestService>();

        return services;
    }
}
