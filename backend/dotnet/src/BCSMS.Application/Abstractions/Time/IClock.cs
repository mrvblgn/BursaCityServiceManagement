namespace BCSMS.Application.Abstractions.Time;

/// <summary>
/// Abstraction for providing the current UTC time.
/// Keeps application and domain operations deterministic and testable.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
