using BCSMS.Application.Abstractions.Security;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Domain.Entities;

namespace BCSMS.UnitTests.Fakes;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IClock _clock;

    public FakeJwtTokenGenerator(IClock clock)
    {
        _clock = clock;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var expiresAt = _clock.UtcNow.AddMinutes(60);
        var token = $"fake_jwt_token_for_{user.Id}";
        return (token, expiresAt);
    }
}
