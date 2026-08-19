using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.ServiceRequests.Manager;
using BCSMS.Application.ServiceRequests.Manager.Assign;
using BCSMS.Application.ServiceRequests.Manager.Close;
using BCSMS.Application.ServiceRequests.Manager.GetMunicipal;
using BCSMS.Application.ServiceRequests.Manager.Reject;
using BCSMS.Application.ServiceRequests.Manager.Reopen;
using BCSMS.Domain.Common;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;
using BCSMS.UnitTests.Fakes;
using Xunit;

namespace BCSMS.UnitTests.Manager;

public class ManagerServiceRequestServiceTests
{
    private readonly FakeServiceRequestRepository _requestRepository;
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeDepartmentRepository _departmentRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeClock _clock = new();
    private readonly ManagerServiceRequestService _service;

    private readonly User _manager;
    private readonly Department _department;
    private readonly User _employee;
    private readonly Category _category;

    public ManagerServiceRequestServiceTests()
    {
        _requestRepository = new FakeServiceRequestRepository(_categoryRepository, _departmentRepository, _userRepository);

        _service = new ManagerServiceRequestService(
            _requestRepository,
            _userRepository,
            _departmentRepository,
            _clock);

        var deptId = Guid.NewGuid();
        _department = new Department(deptId, "Fen İşleri", null, _clock.UtcNow);
        _departmentRepository.Add(_department);

        _manager = new User(
            Guid.NewGuid(),
            new FullName("Kemal", "Yilmaz"),
            new ContactInfo("manager@bursa.bel.tr"),
            "hashed_pwd",
            UserRole.Manager,
            deptId,
            _clock.UtcNow);
        _userRepository.Add(_manager);

        _employee = new User(
            Guid.NewGuid(),
            new FullName("Ahmet", "Usta"),
            new ContactInfo("employee@bursa.bel.tr"),
            "hashed_pwd",
            UserRole.Employee,
            deptId,
            _clock.UtcNow);
        _userRepository.Add(_employee);

        _category = new Category(Guid.NewGuid(), "Road", null, _clock.UtcNow);
        _categoryRepository.Add(_category);
    }

    [Fact]
    public async Task GetMunicipalRequestsAsync_WithValidManager_ShouldReturnPagedResults()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Pothole", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        var query = new GetMunicipalRequestsQuery(_manager.Id);
        var result = await _service.GetMunicipalRequestsAsync(query);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(request.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetMunicipalRequestsAsync_WithNonManager_ShouldThrowForbiddenException()
    {
        var citizen = new User(
            Guid.NewGuid(),
            new FullName("Ali", "Veli"),
            new ContactInfo("ali@bursa.bel.tr"),
            "hashed_pwd",
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        _userRepository.Add(citizen);

        var query = new GetMunicipalRequestsQuery(citizen.Id);
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetMunicipalRequestsAsync(query));
    }

    [Fact]
    public async Task GetMunicipalRequestsAsync_WithInactiveManager_ShouldThrowApplicationConflictException()
    {
        _manager.Deactivate(_clock.UtcNow);

        var query = new GetMunicipalRequestsQuery(_manager.Id);
        await Assert.ThrowsAsync<ApplicationConflictException>(() => _service.GetMunicipalRequestsAsync(query));
    }

    [Fact]
    public async Task StartReviewAsync_WithValidNewRequest_ShouldTransitionToReviewing()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Broken road", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        await _service.StartReviewAsync(request.Id, _manager.Id);

        var updated = await _requestRepository.GetByIdAsync(request.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.Reviewing, updated.Status);
        Assert.Single(updated.StatusHistory);
        Assert.Equal(RequestStatus.New, updated.StatusHistory[0].OldStatus);
        Assert.Equal(RequestStatus.Reviewing, updated.StatusHistory[0].NewStatus);
        Assert.Equal(_manager.Id, updated.StatusHistory[0].ChangedByUserId);
    }

    [Fact]
    public async Task StartReviewAsync_WithNonNewRequest_ShouldThrowDomainException()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Broken road", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        await Assert.ThrowsAsync<DomainException>(() => _service.StartReviewAsync(request.Id, _manager.Id));
    }

    [Fact]
    public async Task AssignRequestAsync_WithValidDepartmentAndEmployee_ShouldTransitionToAssigned()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Broken road", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        var command = new AssignRequestCommand(
            request.Id,
            _department.Id,
            _employee.Id,
            Priority.High,
            _manager.Id);

        await _service.AssignRequestAsync(command);

        var updated = await _requestRepository.GetByIdAsync(request.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.Assigned, updated.Status);
        Assert.Equal(_department.Id, updated.AssignedDepartmentId);
        Assert.Equal(_employee.Id, updated.AssignedEmployeeId);
        Assert.Equal(Priority.High, updated.Priority);
    }

    [Fact]
    public async Task AssignRequestAsync_WithMismatchedEmployeeDepartment_ShouldThrowApplicationConflictException()
    {
        var otherDept = new Department(Guid.NewGuid(), "Parklar", null, _clock.UtcNow);
        _departmentRepository.Add(otherDept);

        var request = new ServiceRequest(Guid.NewGuid(), "Broken road", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        // _employee belongs to _department, but assigning with otherDept
        var command = new AssignRequestCommand(
            request.Id,
            otherDept.Id,
            _employee.Id,
            Priority.Medium,
            _manager.Id);

        var ex = await Assert.ThrowsAsync<ApplicationConflictException>(() => _service.AssignRequestAsync(command));
        Assert.Equal("The assigned employee does not belong to the selected department.", ex.Message);
    }

    [Fact]
    public async Task AssignRequestAsync_WithInactiveEmployee_ShouldThrowApplicationConflictException()
    {
        _employee.Deactivate(_clock.UtcNow);

        var request = new ServiceRequest(Guid.NewGuid(), "Broken road", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        var command = new AssignRequestCommand(
            request.Id,
            _department.Id,
            _employee.Id,
            Priority.Low,
            _manager.Id);

        await Assert.ThrowsAsync<ApplicationConflictException>(() => _service.AssignRequestAsync(command));
    }

    [Fact]
    public async Task AssignRequestAsync_WithInactiveDepartment_ShouldThrowApplicationConflictException()
    {
        _department.Deactivate(_clock.UtcNow);

        var request = new ServiceRequest(Guid.NewGuid(), "Broken road", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        var command = new AssignRequestCommand(
            request.Id,
            _department.Id,
            _employee.Id,
            Priority.Low,
            _manager.Id);

        await Assert.ThrowsAsync<ApplicationConflictException>(() => _service.AssignRequestAsync(command));
    }

    [Fact]
    public async Task RejectRequestAsync_WithValidNewRequest_ShouldTransitionToRejected()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Out of scope", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        var command = new RejectRequestCommand(request.Id, "Not in municipal boundary", _manager.Id);
        await _service.RejectRequestAsync(command);

        var updated = await _requestRepository.GetByIdAsync(request.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.Rejected, updated.Status);
    }

    [Fact]
    public async Task CloseRequestAsync_WithResolvedRequest_ShouldTransitionToClosed()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Fixed pothole", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        request.Assign(_department.Id, _employee.Id, Priority.Medium, _manager.Id, _clock.UtcNow);
        request.StartProgress(_employee.Id, _clock.UtcNow);
        request.Resolve(_employee.Id, _clock.UtcNow, "Repaired");
        _requestRepository.Requests.Add(request);

        var command = new CloseRequestCommand(request.Id, "Inspected and confirmed", _manager.Id);
        await _service.CloseRequestAsync(command);

        var updated = await _requestRepository.GetByIdAsync(request.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.Closed, updated.Status);
    }

    [Fact]
    public async Task ReopenRequestAsync_WithResolvedRequest_ShouldTransitionToInProgress()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Pothole not fixed completely", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        request.Assign(_department.Id, _employee.Id, Priority.Medium, _manager.Id, _clock.UtcNow);
        request.StartProgress(_employee.Id, _clock.UtcNow);
        request.Resolve(_employee.Id, _clock.UtcNow, "Repaired");
        _requestRepository.Requests.Add(request);

        var command = new ReopenRequestCommand(request.Id, "Surface still uneven", _manager.Id);
        await _service.ReopenRequestAsync(command);

        var updated = await _requestRepository.GetByIdAsync(request.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.InProgress, updated.Status);
    }

    [Fact]
    public async Task CloseRequestAsync_WithNonResolvedRequest_ShouldThrowDomainException()
    {
        var request = new ServiceRequest(Guid.NewGuid(), "Still working", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        request.StartReview(_manager.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        var command = new CloseRequestCommand(request.Id, "Close now", _manager.Id);
        await Assert.ThrowsAsync<DomainException>(() => _service.CloseRequestAsync(command));
    }
}
