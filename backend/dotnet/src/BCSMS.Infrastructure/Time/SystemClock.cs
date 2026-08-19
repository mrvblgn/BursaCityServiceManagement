using BCSMS.Application.Abstractions.Time;

namespace BCSMS.Infrastructure.Time;

/// <summary>
/// Default system clock providing current UTC time.
/// </summary>
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
