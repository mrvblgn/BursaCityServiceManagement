using BCSMS.Domain.Common;
using BCSMS.Domain.Enums;

namespace BCSMS.Domain.Entities;

/// <summary>
/// Records a single status transition on a service request.
/// Child entity of the ServiceRequest aggregate — cannot be created externally.
/// </summary>
public class StatusHistoryEntry : BaseEntity
{
    public Guid ServiceRequestId { get; private set; }
    public RequestStatus OldStatus { get; private set; }
    public RequestStatus NewStatus { get; private set; }
    public string? Note { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private StatusHistoryEntry() : base()
    {
        // For EF Core
    }

    internal StatusHistoryEntry(
        Guid id,
        Guid serviceRequestId,
        RequestStatus oldStatus,
        RequestStatus newStatus,
        Guid changedByUserId,
        DateTime changedAt,
        string? note = null)
        : base(id)
    {
        ServiceRequestId = serviceRequestId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        ChangedAt = changedAt;
        Note = note?.Trim();
    }
}
