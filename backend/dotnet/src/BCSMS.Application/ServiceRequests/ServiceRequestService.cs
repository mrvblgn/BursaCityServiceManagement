using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;

namespace BCSMS.Application.ServiceRequests;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IClock _clock;

    public ServiceRequestService(
        IServiceRequestRepository serviceRequestRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IClock clock)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _clock = clock;
    }

    public async Task<CreateServiceRequestResponse> CreateAsync(
        CreateServiceRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ValidationException("Title is required.");

        if (command.Title.Length > 300)
            throw new ValidationException("Title cannot exceed 300 characters.");

        if (command.CitizenId == Guid.Empty)
            throw new ValidationException("Citizen ID is required.");

        if (command.CategoryId == Guid.Empty)
            throw new ValidationException("Category ID is required.");

        // Check coordinate consistency
        if (command.Latitude.HasValue != command.Longitude.HasValue)
            throw new ValidationException("Both latitude and longitude must be provided together.");

        // Verify citizen exists and is active
        var citizen = await _userRepository.GetByIdAsync(command.CitizenId, cancellationToken);
        if (citizen is null)
            throw new NotFoundException("User", command.CitizenId);

        if (!citizen.IsActive)
            throw new ApplicationConflictException("Citizen account is deactivated.");

        if (citizen.Role != UserRole.Citizen)
            throw new ForbiddenException("Only citizens can submit municipal service requests.");

        // Verify category exists and is active
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
            throw new NotFoundException("Category", command.CategoryId);

        if (!category.IsActive)
            throw new ApplicationConflictException("The selected category is inactive.");

        // Create Location value object if coordinates provided
        Location? location = null;
        if (command.Latitude.HasValue && command.Longitude.HasValue)
        {
            location = new Location(command.Latitude.Value, command.Longitude.Value, command.AddressText);
        }

        var now = _clock.UtcNow;
        var serviceRequest = new ServiceRequest(
            Guid.NewGuid(),
            command.Title,
            command.CategoryId,
            command.CitizenId,
            now,
            command.Description,
            location);

        await _serviceRequestRepository.AddAsync(serviceRequest, cancellationToken);

        return new CreateServiceRequestResponse(
            serviceRequest.Id,
            serviceRequest.Title,
            serviceRequest.CategoryId,
            serviceRequest.CitizenId,
            serviceRequest.Status,
            serviceRequest.CreatedAt);
    }

    public async Task<PagedResult<ServiceRequestSummaryDto>> GetMyRequestsAsync(
        GetMyServiceRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ValidationException("Query cannot be null.");

        if (query.CitizenId == Guid.Empty)
            throw new ValidationException("Citizen ID is required.");

        if (query.PageNumber < 1)
            throw new ValidationException("Page number must be greater than or equal to 1.");

        if (query.PageSize < 1 || query.PageSize > 100)
            throw new ValidationException("Page size must be between 1 and 100.");

        return await _serviceRequestRepository.GetSummariesByCitizenIdAsync(
            query.CitizenId,
            query.StatusFilter,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }

    public async Task<ServiceRequestDetailDto> GetByIdAsync(
        GetServiceRequestByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ValidationException("Query cannot be null.");

        if (query.RequestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        if (query.RequestingUserId == Guid.Empty)
            throw new ValidationException("Requesting user ID is required.");

        var detail = await _serviceRequestRepository.GetDetailByIdAsync(query.RequestId, cancellationToken);
        if (detail is null)
            throw new NotFoundException("ServiceRequest", query.RequestId);

        if (detail.CitizenId != query.RequestingUserId)
            throw new ForbiddenException("You do not have permission to view this service request.");

        return detail;
    }
}
