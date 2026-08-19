namespace BCSMS.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a service request.
/// </summary>
public enum RequestStatus
{
    New = 0,
    Reviewing = 1,
    Assigned = 2,
    InProgress = 3,
    Resolved = 4,
    Closed = 5,
    Rejected = 6,
    Cancelled = 7
}
