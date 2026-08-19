using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.ServiceRequests.Employee;
using BCSMS.Application.ServiceRequests.Employee.GetAssigned;
using BCSMS.Application.ServiceRequests.Employee.Resolve;
using BCSMS.Domain.Common;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;
using BCSMS.UnitTests.Fakes;
using Xunit;

namespace BCSMS.UnitTests.Employee;

public class EmployeeServiceRequestServiceTests
{
    private readonly FakeServiceRequestRepository _requestRepository;
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeDepartmentRepository _departmentRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeClock _clock = new();
    private readonly EmployeeServiceRequestService _service;

    private readonly User _employee1;
    private readonly User _employee2;
    private readonly Department _department;
    private readonly Category _category;

    public EmployeeServiceRequestServiceTests()
    {
        _requestRepository = new FakeServiceRequestRepository(_categoryRepository, _departmentRepository, _userRepository);

        _service = new EmployeeServiceRequestService(
            _requestRepository,
            _userRepository,
            _clock);

        var deptId = Guid.NewGuid();
        _department = new Department(deptId, "Temizlik İşleri", null, _clock.UtcNow);
        _departmentRepository.Add(_department);

        _employee1 = new User(
            Guid.NewGuid(),
            new FullName("Ali", "Temiz"),
            new ContactInfo("ali@bursa.bel.tr"),
            "hashed_pwd",
            UserRole.Employee,
            deptId,
            _clock.UtcNow);
        _userRepository.Add(_employee1);

        _employee2 = new User(
            Guid.NewGuid(),
            new FullName("Veli", "Temiz"),
            new ContactInfo("veli@bursa.bel.tr"),
            "hashed_pwd",
            UserRole.Employee,
            deptId,
            _clock.UtcNow);
        _userRepository.Add(_employee2);

        _category = new Category(Guid.NewGuid(), "Waste", null, _clock.UtcNow);
        _categoryRepository.Add(_category);
    }

    [Fact]
    public async Task GetMyAssignedRequestsAsync_WithValidEmployee_ShouldReturnAssignedRequestsOnly()
    {
        var req1 = new ServiceRequest(Guid.NewGuid(), "Trash 1", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req1.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req1.Assign(_department.Id, _employee1.Id, Priority.Medium, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(req1);

        var req2 = new ServiceRequest(Guid.NewGuid(), "Trash 2", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req2.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req2.Assign(_department.Id, _employee2.Id, Priority.High, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(req2);

        var query = new GetMyAssignedRequestsQuery(_employee1.Id);
        var result = await _service.GetMyAssignedRequestsAsync(query);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(req1.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetMyAssignedRequestsAsync_WithNonEmployee_ShouldThrowForbiddenException()
    {
        var citizen = new User(
            Guid.NewGuid(),
            new FullName("Citizen", "User"),
            new ContactInfo("citizen@bursa.bel.tr"),
            "hashed_pwd",
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        _userRepository.Add(citizen);

        var query = new GetMyAssignedRequestsQuery(citizen.Id);
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetMyAssignedRequestsAsync(query));
    }

    [Fact]
    public async Task GetAssignedRequestByIdAsync_WhenAssignedToEmployee_ShouldReturnDetail()
    {
        var req = new ServiceRequest(Guid.NewGuid(), "Trash pickup", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req.Assign(_department.Id, _employee1.Id, Priority.Medium, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(req);

        var detail = await _service.GetAssignedRequestByIdAsync(req.Id, _employee1.Id);

        Assert.NotNull(detail);
        Assert.Equal(req.Id, detail.Id);
        Assert.Equal(_employee1.Id, detail.AssignedEmployeeId);
    }

    [Fact]
    public async Task GetAssignedRequestByIdAsync_WhenAssignedToDifferentEmployee_ShouldThrowForbiddenException()
    {
        var req = new ServiceRequest(Guid.NewGuid(), "Trash pickup", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req.Assign(_department.Id, _employee1.Id, Priority.Medium, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(req);

        // employee2 tries to access request assigned to employee1
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetAssignedRequestByIdAsync(req.Id, _employee2.Id));
    }

    [Fact]
    public async Task StartWorkAsync_WhenAssignedToEmployee_ShouldTransitionToInProgress()
    {
        var req = new ServiceRequest(Guid.NewGuid(), "Trash pickup", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req.Assign(_department.Id, _employee1.Id, Priority.Medium, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(req);

        await _service.StartWorkAsync(req.Id, _employee1.Id);

        var updated = await _requestRepository.GetByIdAsync(req.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.InProgress, updated.Status);
    }

    [Fact]
    public async Task StartWorkAsync_WhenAssignedToDifferentEmployee_ShouldThrowForbiddenException()
    {
        var req = new ServiceRequest(Guid.NewGuid(), "Trash pickup", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req.Assign(_department.Id, _employee1.Id, Priority.Medium, Guid.NewGuid(), _clock.UtcNow);
        _requestRepository.Requests.Add(req);

        // employee2 tries to start work on employee1's request
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.StartWorkAsync(req.Id, _employee2.Id));
    }

    [Fact]
    public async Task ResolveRequestAsync_WhenInProgressAndAssignedToEmployee_ShouldTransitionToResolved()
    {
        var req = new ServiceRequest(Guid.NewGuid(), "Trash pickup", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req.Assign(_department.Id, _employee1.Id, Priority.Medium, Guid.NewGuid(), _clock.UtcNow);
        req.StartProgress(_employee1.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(req);

        var command = new ResolveRequestCommand(req.Id, "Trash cleared", _employee1.Id);
        await _service.ResolveRequestAsync(command);

        var updated = await _requestRepository.GetByIdAsync(req.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.Resolved, updated.Status);
    }

    [Fact]
    public async Task ResolveRequestAsync_WhenAssignedToDifferentEmployee_ShouldThrowForbiddenException()
    {
        var req = new ServiceRequest(Guid.NewGuid(), "Trash pickup", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        req.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req.Assign(_department.Id, _employee1.Id, Priority.Medium, Guid.NewGuid(), _clock.UtcNow);
        req.StartProgress(_employee1.Id, _clock.UtcNow);
        _requestRepository.Requests.Add(req);

        var command = new ResolveRequestCommand(req.Id, "Done", _employee2.Id);
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ResolveRequestAsync(command));
    }

    [Fact]
    public async Task StartWorkAsync_WithNonAssignedStatus_ShouldThrowDomainException()
    {
        var req = new ServiceRequest(Guid.NewGuid(), "Trash pickup", _category.Id, Guid.NewGuid(), _clock.UtcNow);
        // Status is New, not Assigned
        _requestRepository.Requests.Add(req);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.StartWorkAsync(req.Id, _employee1.Id));
    }
}
