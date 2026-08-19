using BCSMS.Application.Auth.Login;
using BCSMS.Application.Auth.Register;

namespace BCSMS.Application.Auth;

/// <summary>
/// Service interface for authentication use cases (registration and login).
/// </summary>
public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);
}
