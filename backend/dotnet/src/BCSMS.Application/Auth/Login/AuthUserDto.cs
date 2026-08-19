using BCSMS.Domain.Enums;

namespace BCSMS.Application.Auth.Login;

/// <summary>
/// Safe user profile summary for authentication responses.
/// </summary>
public record AuthUserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role);
