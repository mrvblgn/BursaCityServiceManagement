using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Employee.GetAssigned;
using BCSMS.Application.ServiceRequests.Employee.Resolve;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Employee;

public class EmployeeServiceRequestService : IEmployeeServiceRequestService
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClock _clock;

    public EmployeeServiceRequestService(
        IServiceRequestRepository requestRepository,
        IUserRepository userRepository,
        IClock clock)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _clock = clock;
    }

    public async Task<PagedResult<EmployeeServiceRequestSummaryDto>> GetMyAssignedRequestsAsync(
        GetMyAssignedRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ValidationException("Query cannot be null.");

        await ValidateEmployeeActorAsync(query.EmployeeUserId, cancellationToken);

        if (query.PageNumber < 1)
            throw new ValidationException("Page number must be greater than or equal to 1.");

        if (query.PageSize < 1 || query.PageSize > 100)
            throw new ValidationException("Page size must be between 1 and 100.");

        return await _requestRepository.GetSummariesByAssignedEmployeeIdAsync(
            query.EmployeeUserId,
            query.Status,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }

    public async Task<ServiceRequestDetailDto> GetAssignedRequestByIdAsync(
        Guid requestId,
        Guid employeeUserId,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateEmployeeActorAsync(employeeUserId, cancellationToken);

        var detail = await _requestRepository.GetDetailByIdAsync(requestId, cancellationToken);
        if (detail == null)
            throw new NotFoundException("Service request not found.");

        if (detail.AssignedEmployeeId != employeeUserId)
            throw new ForbiddenException("You do not have permission to view this service request.");

        return detail;
    }

    public async Task StartWorkAsync(
        Guid requestId,
        Guid employeeUserId,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateEmployeeActorAsync(employeeUserId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
            throw new NotFoundException("Service request not found.");

        if (request.AssignedEmployeeId != employeeUserId)
            throw new ForbiddenException("You are not assigned to this service request.");

        request.StartProgress(employeeUserId, _clock.UtcNow);

        await _requestRepository.UpdateAsync(request, cancellationToken);
    }

    public async Task ResolveRequestAsync(
        ResolveRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (command.RequestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateEmployeeActorAsync(command.EmployeeUserId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request == null)
            throw new NotFoundException("Service request not found.");

        if (request.AssignedEmployeeId != command.EmployeeUserId)
            throw new ForbiddenException("You are not assigned to this service request.");

        request.Resolve(command.EmployeeUserId, _clock.UtcNow, command.Note);

        await _requestRepository.UpdateAsync(request, cancellationToken);
    }

    private async Task ValidateEmployeeActorAsync(Guid employeeUserId, CancellationToken cancellationToken)
    {
        if (employeeUserId == Guid.Empty)
            throw new ValidationException("Employee user ID is required.");

        var employee = await _userRepository.GetByIdAsync(employeeUserId, cancellationToken);
        if (employee == null)
            throw new NotFoundException("Employee user not found.");

        if (!employee.IsActive)
            throw new ApplicationConflictException("User account is inactive.");

        if (employee.Role != UserRole.Employee)
            throw new ForbiddenException("Only employees have permission to perform this action.");
    }
}
