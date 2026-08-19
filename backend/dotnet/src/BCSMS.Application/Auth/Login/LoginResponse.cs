namespace BCSMS.Application.Auth.Login;

/// <summary>
/// Response returned upon successful authentication.
/// </summary>
public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    AuthUserDto User);
