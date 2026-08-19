namespace BCSMS.Infrastructure.Security;

/// <summary>
/// Configuration options for JSON Web Token generation and verification.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = "BCSMS.API";
    public string Audience { get; set; } = "BCSMS.Web";
    public string Secret { get; set; } = "BursaCityServiceManagementSystemSafeDevelopmentKey2026!";
    public int ExpiryMinutes { get; set; } = 120;
}
