using BCSMS.Application.Abstractions.Time;

namespace BCSMS.UnitTests.Fakes;

public class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = new DateTime(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);
}
