using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Common;

/// <summary>
/// DTO representing a historical status change entry.
/// </summary>
public record StatusHistoryEntryDto(
    Guid Id,
    RequestStatus OldStatus,
    RequestStatus NewStatus,
    string? Note,
    Guid ChangedByUserId,
    DateTime ChangedAt);
