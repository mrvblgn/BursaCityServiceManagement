using BCSMS.Application.ServiceRequests.Common;
using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.GetById;

/// <summary>
/// Detailed DTO for a service request including history, comments, and attachments.
/// </summary>
public record ServiceRequestDetailDto(
    Guid Id,
    string Title,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    RequestStatus Status,
    Priority? Priority,
    LocationDto? Location,
    Guid CitizenId,
    Guid? AssignedDepartmentId,
    Guid? AssignedEmployeeId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<StatusHistoryEntryDto> StatusHistory,
    IReadOnlyList<CommentDto> Comments,
    IReadOnlyList<AttachmentDto> Attachments);
