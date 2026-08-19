using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Common;
using BCSMS.Application.ServiceRequests.Employee.GetAssigned;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Application.ServiceRequests.Manager.GetMunicipal;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;

namespace BCSMS.UnitTests.Fakes;

public class FakeServiceRequestRepository : IServiceRequestRepository
{
    public readonly List<ServiceRequest> Requests = new();
    private readonly FakeCategoryRepository? _categoryRepository;
    private readonly FakeDepartmentRepository? _departmentRepository;
    private readonly FakeUserRepository? _userRepository;

    public FakeServiceRequestRepository(
        FakeCategoryRepository? categoryRepository = null,
        FakeDepartmentRepository? departmentRepository = null,
        FakeUserRepository? userRepository = null)
    {
        _categoryRepository = categoryRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
    }

    public Task AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        Requests.Add(serviceRequest);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        var existingIndex = Requests.FindIndex(r => r.Id == serviceRequest.Id);
        if (existingIndex >= 0)
        {
            Requests[existingIndex] = serviceRequest;
        }
        else
        {
            Requests.Add(serviceRequest);
        }
        return Task.CompletedTask;
    }

    public Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = Requests.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(item);
    }

    public Task<ServiceRequestDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = Requests.FirstOrDefault(x => x.Id == id);
        if (r == null)
            return Task.FromResult<ServiceRequestDetailDto?>(null);

        var categoryName = "Test Category";
        if (_categoryRepository != null)
        {
            var cat = _categoryRepository.GetByIdAsync(r.CategoryId, cancellationToken).GetAwaiter().GetResult();
            if (cat != null)
                categoryName = cat.Name;
        }

        var detail = new ServiceRequestDetailDto(
            r.Id,
            r.Title,
            r.Description,
            r.CategoryId,
            categoryName,
            r.Status,
            r.Priority,
            r.Location != null ? new LocationDto(r.Location.Latitude, r.Location.Longitude, r.Location.AddressText) : null,
            r.CitizenId,
            r.AssignedDepartmentId,
            r.AssignedEmployeeId,
            r.CreatedAt,
            r.UpdatedAt,
            r.StatusHistory.Select(h => new StatusHistoryEntryDto(h.Id, h.OldStatus, h.NewStatus, h.Note, h.ChangedByUserId, h.ChangedAt)).ToList(),
            r.Comments.Select(c => new CommentDto(c.Id, c.Content, c.CreatedByUserId, c.CreatedAt)).ToList(),
            r.Attachments.Select(a => new AttachmentDto(a.Id, a.FileName, a.ContentType, a.FileSizeInBytes, a.StoragePath, a.UploadedByUserId, a.UploadedAt)).ToList()
        );

        return Task.FromResult<ServiceRequestDetailDto?>(detail);
    }

    public Task<PagedResult<ServiceRequestSummaryDto>> GetSummariesByCitizenIdAsync(
        Guid citizenId,
        RequestStatus? statusFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Requests.Where(r => r.CitizenId == citizenId);

        if (statusFilter.HasValue)
        {
            query = query.Where(r => r.Status == statusFilter.Value);
        }

        var list = query.OrderByDescending(r => r.CreatedAt).ToList();
        var totalCount = list.Count;

        var pagedItems = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r =>
            {
                var categoryName = "Test Category";
                if (_categoryRepository != null)
                {
                    var cat = _categoryRepository.GetByIdAsync(r.CategoryId, cancellationToken).GetAwaiter().GetResult();
                    if (cat != null)
                        categoryName = cat.Name;
                }

                return new ServiceRequestSummaryDto(
                    r.Id,
                    r.Title,
                    r.CategoryId,
                    categoryName,
                    r.Status,
                    r.Priority,
                    r.Location != null ? new LocationDto(r.Location.Latitude, r.Location.Longitude, r.Location.AddressText) : null,
                    r.CreatedAt);
            })
            .ToList();

        var result = new PagedResult<ServiceRequestSummaryDto>(pagedItems, totalCount, pageNumber, pageSize);
        return Task.FromResult(result);
    }

    public Task<PagedResult<MunicipalServiceRequestSummaryDto>> GetMunicipalSummariesAsync(
        RequestStatus? statusFilter,
        Guid? categoryId,
        Guid? departmentId,
        Priority? priority,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Requests.AsEnumerable();

        if (statusFilter.HasValue)
            query = query.Where(r => r.Status == statusFilter.Value);

        if (categoryId.HasValue)
            query = query.Where(r => r.CategoryId == categoryId.Value);

        if (departmentId.HasValue)
            query = query.Where(r => r.AssignedDepartmentId == departmentId.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        var list = query.OrderByDescending(r => r.CreatedAt).ToList();
        var totalCount = list.Count;

        var pagedItems = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r =>
            {
                var categoryName = "Test Category";
                if (_categoryRepository != null)
                {
                    var cat = _categoryRepository.GetByIdAsync(r.CategoryId, cancellationToken).GetAwaiter().GetResult();
                    if (cat != null)
                        categoryName = cat.Name;
                }

                var citizenName = "Test Citizen";
                if (_userRepository != null)
                {
                    var citizen = _userRepository.GetByIdAsync(r.CitizenId, cancellationToken).GetAwaiter().GetResult();
                    if (citizen != null)
                        citizenName = $"{citizen.Name.FirstName} {citizen.Name.LastName}";
                }

                string? deptName = null;
                if (r.AssignedDepartmentId.HasValue && _departmentRepository != null)
                {
                    var dept = _departmentRepository.GetByIdAsync(r.AssignedDepartmentId.Value, cancellationToken).GetAwaiter().GetResult();
                    deptName = dept?.Name;
                }

                string? empName = null;
                if (r.AssignedEmployeeId.HasValue && _userRepository != null)
                {
                    var emp = _userRepository.GetByIdAsync(r.AssignedEmployeeId.Value, cancellationToken).GetAwaiter().GetResult();
                    if (emp != null)
                        empName = $"{emp.Name.FirstName} {emp.Name.LastName}";
                }

                return new MunicipalServiceRequestSummaryDto(
                    r.Id,
                    r.Title,
                    r.CategoryId,
                    categoryName,
                    r.CitizenId,
                    citizenName,
                    r.Status,
                    r.Priority,
                    r.AssignedDepartmentId,
                    deptName,
                    r.AssignedEmployeeId,
                    empName,
                    r.Location != null ? new LocationDto(r.Location.Latitude, r.Location.Longitude, r.Location.AddressText) : null,
                    r.CreatedAt);
            })
            .ToList();

        var result = new PagedResult<MunicipalServiceRequestSummaryDto>(pagedItems, totalCount, pageNumber, pageSize);
        return Task.FromResult(result);
    }

    public Task<PagedResult<EmployeeServiceRequestSummaryDto>> GetSummariesByAssignedEmployeeIdAsync(
        Guid employeeId,
        RequestStatus? statusFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Requests.Where(r => r.AssignedEmployeeId == employeeId);

        if (statusFilter.HasValue)
            query = query.Where(r => r.Status == statusFilter.Value);

        var list = query.OrderByDescending(r => r.CreatedAt).ToList();
        var totalCount = list.Count;

        var pagedItems = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r =>
            {
                var categoryName = "Test Category";
                if (_categoryRepository != null)
                {
                    var cat = _categoryRepository.GetByIdAsync(r.CategoryId, cancellationToken).GetAwaiter().GetResult();
                    if (cat != null)
                        categoryName = cat.Name;
                }

                return new EmployeeServiceRequestSummaryDto(
                    r.Id,
                    r.Title,
                    r.CategoryId,
                    categoryName,
                    r.Status,
                    r.Priority,
                    r.Location != null ? new LocationDto(r.Location.Latitude, r.Location.Longitude, r.Location.AddressText) : null,
                    r.CreatedAt);
            })
            .ToList();

        var result = new PagedResult<EmployeeServiceRequestSummaryDto>(pagedItems, totalCount, pageNumber, pageSize);
        return Task.FromResult(result);
    }
}
