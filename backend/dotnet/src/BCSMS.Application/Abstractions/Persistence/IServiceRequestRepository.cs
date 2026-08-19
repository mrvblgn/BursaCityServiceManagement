using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;

namespace BCSMS.Application.Abstractions.Persistence;

/// <summary>
/// Repository abstraction for ServiceRequest aggregate root.
/// Supports both aggregate root operations and optimized read-model projections.
/// </summary>
public interface IServiceRequestRepository
{
    /// <summary>
    /// Adds and persists a new ServiceRequest aggregate root.
    /// </summary>
    Task AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the ServiceRequest aggregate root by ID for domain operations.
    /// </summary>
    Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a detailed read-only DTO for a service request in a single efficient query.
    /// </summary>
    Task<ServiceRequestDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves paginated summary DTOs for a citizen's service requests with category name.
    /// </summary>
    Task<PagedResult<ServiceRequestSummaryDto>> GetSummariesByCitizenIdAsync(
        Guid citizenId,
        RequestStatus? statusFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
