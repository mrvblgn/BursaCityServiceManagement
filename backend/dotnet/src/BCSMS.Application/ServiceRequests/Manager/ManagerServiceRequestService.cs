using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.Manager.Assign;
using BCSMS.Application.ServiceRequests.Manager.Close;
using BCSMS.Application.ServiceRequests.Manager.GetMunicipal;
using BCSMS.Application.ServiceRequests.Manager.Reject;
using BCSMS.Application.ServiceRequests.Manager.Reopen;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Manager;

public class ManagerServiceRequestService : IManagerServiceRequestService
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IClock _clock;

    public ManagerServiceRequestService(
        IServiceRequestRepository requestRepository,
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        IClock clock)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _clock = clock;
    }

    public async Task<PagedResult<MunicipalServiceRequestSummaryDto>> GetMunicipalRequestsAsync(
        GetMunicipalRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ValidationException("Query cannot be null.");

        await ValidateManagerActorAsync(query.ManagerUserId, cancellationToken);

        if (query.PageNumber < 1)
            throw new ValidationException("Page number must be greater than or equal to 1.");

        if (query.PageSize < 1 || query.PageSize > 100)
            throw new ValidationException("Page size must be between 1 and 100.");

        return await _requestRepository.GetMunicipalSummariesAsync(
            query.Status,
            query.CategoryId,
            query.DepartmentId,
            query.Priority,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }

    public async Task<ServiceRequestDetailDto> GetMunicipalRequestByIdAsync(
        Guid requestId,
        Guid managerUserId,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateManagerActorAsync(managerUserId, cancellationToken);

        var detail = await _requestRepository.GetDetailByIdAsync(requestId, cancellationToken);
        if (detail == null)
            throw new NotFoundException("Service request not found.");

        return detail;
    }

    public async Task StartReviewAsync(
        Guid requestId,
        Guid managerUserId,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateManagerActorAsync(managerUserId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
            throw new NotFoundException("Service request not found.");

        request.StartReview(managerUserId, _clock.UtcNow);

        await _requestRepository.UpdateAsync(request, cancellationToken);
    }

    public async Task AssignRequestAsync(
        AssignRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (command.RequestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        if (command.DepartmentId == Guid.Empty)
            throw new ValidationException("Department ID is required.");

        if (command.EmployeeId == Guid.Empty)
            throw new ValidationException("Employee ID is required.");

        await ValidateManagerActorAsync(command.ManagerUserId, cancellationToken);

        // Validate Department
        var department = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (department == null)
            throw new NotFoundException("Department not found.");

        if (!department.IsActive)
            throw new ApplicationConflictException("Cannot assign request to an inactive department.");

        // Validate Employee
        var employee = await _userRepository.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee == null)
            throw new NotFoundException("Employee not found.");

        if (!employee.IsActive)
            throw new ApplicationConflictException("Cannot assign request to an inactive employee.");

        if (employee.Role != UserRole.Employee)
            throw new ApplicationConflictException("Selected user is not an employee.");

        if (employee.DepartmentId != command.DepartmentId)
            throw new ApplicationConflictException("The assigned employee does not belong to the selected department.");

        // Load aggregate and assign
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request == null)
            throw new NotFoundException("Service request not found.");

        request.Assign(
            command.DepartmentId,
            command.EmployeeId,
            command.Priority,
            command.ManagerUserId,
            _clock.UtcNow);

        await _requestRepository.UpdateAsync(request, cancellationToken);
    }

    public async Task RejectRequestAsync(
        RejectRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (command.RequestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateManagerActorAsync(command.ManagerUserId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request == null)
            throw new NotFoundException("Service request not found.");

        request.Reject(command.ManagerUserId, _clock.UtcNow, command.Note);

        await _requestRepository.UpdateAsync(request, cancellationToken);
    }

    public async Task CloseRequestAsync(
        CloseRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (command.RequestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateManagerActorAsync(command.ManagerUserId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request == null)
            throw new NotFoundException("Service request not found.");

        request.Close(command.ManagerUserId, _clock.UtcNow, command.Note);

        await _requestRepository.UpdateAsync(request, cancellationToken);
    }

    public async Task ReopenRequestAsync(
        ReopenRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ValidationException("Command cannot be null.");

        if (command.RequestId == Guid.Empty)
            throw new ValidationException("Request ID is required.");

        await ValidateManagerActorAsync(command.ManagerUserId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request == null)
            throw new NotFoundException("Service request not found.");

        request.Reopen(command.ManagerUserId, _clock.UtcNow, command.Note);

        await _requestRepository.UpdateAsync(request, cancellationToken);
    }

    private async Task ValidateManagerActorAsync(Guid managerUserId, CancellationToken cancellationToken)
    {
        if (managerUserId == Guid.Empty)
            throw new ValidationException("Manager user ID is required.");

        var manager = await _userRepository.GetByIdAsync(managerUserId, cancellationToken);
        if (manager == null)
            throw new NotFoundException("Manager user not found.");

        if (!manager.IsActive)
            throw new ApplicationConflictException("User account is inactive.");

        if (manager.Role != UserRole.Manager)
            throw new ForbiddenException("Only managers have permission to perform this action.");
    }
}
