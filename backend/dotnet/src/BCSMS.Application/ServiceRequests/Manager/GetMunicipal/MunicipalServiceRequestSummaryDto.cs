using BCSMS.Application.ServiceRequests.Common;
using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Manager.GetMunicipal;

/// <summary>
/// Summary projection of a municipal service request for Manager dashboard list views.
/// Includes joined Category, Citizen, Department, and Employee names in a single query.
/// </summary>
public record MunicipalServiceRequestSummaryDto(
    Guid Id,
    string Title,
    Guid CategoryId,
    string CategoryName,
    Guid CitizenId,
    string CitizenName,
    RequestStatus Status,
    Priority? Priority,
    Guid? AssignedDepartmentId,
    string? AssignedDepartmentName,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    LocationDto? Location,
    DateTime CreatedAt);
