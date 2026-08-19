using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Common;
using BCSMS.Application.ServiceRequests.Employee.GetAssigned;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Application.ServiceRequests.Manager.GetMunicipal;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BCSMS.Infrastructure.Persistence.Repositories;

public class ServiceRequestRepository : IServiceRequestRepository
{
    private readonly BcsmsDbContext _dbContext;

    public ServiceRequestRepository(BcsmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.ServiceRequests.AddAsync(serviceRequest, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        _dbContext.ServiceRequests.Update(serviceRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .Include(r => r.StatusHistory)
            .Include(r => r.Comments)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ServiceRequestDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = from r in _dbContext.ServiceRequests.AsNoTracking()
                    where r.Id == id
                    join c in _dbContext.Categories.AsNoTracking() on r.CategoryId equals c.Id
                    select new ServiceRequestDetailDto(
                        r.Id,
                        r.Title,
                        r.Description,
                        r.CategoryId,
                        c.Name,
                        r.Status,
                        r.Priority,
                        r.Location != null
                            ? new LocationDto(r.Location.Latitude, r.Location.Longitude, r.Location.AddressText)
                            : null,
                        r.CitizenId,
                        r.AssignedDepartmentId,
                        r.AssignedEmployeeId,
                        r.CreatedAt,
                        r.UpdatedAt,
                        r.StatusHistory
                            .OrderBy(h => h.ChangedAt)
                            .Select(h => new StatusHistoryEntryDto(
                                h.Id,
                                h.OldStatus,
                                h.NewStatus,
                                h.Note,
                                h.ChangedByUserId,
                                h.ChangedAt))
                            .ToList(),
                        r.Comments
                            .OrderBy(cm => cm.CreatedAt)
                            .Select(cm => new CommentDto(
                                cm.Id,
                                cm.Content,
                                cm.CreatedByUserId,
                                cm.CreatedAt))
                            .ToList(),
                        r.Attachments
                            .OrderBy(a => a.UploadedAt)
                            .Select(a => new AttachmentDto(
                                a.Id,
                                a.FileName,
                                a.ContentType,
                                a.FileSizeInBytes,
                                a.StoragePath,
                                a.UploadedByUserId,
                                a.UploadedAt))
                            .ToList());

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<ServiceRequestSummaryDto>> GetSummariesByCitizenIdAsync(
        Guid citizenId,
        RequestStatus? statusFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = from r in _dbContext.ServiceRequests.AsNoTracking()
                        where r.CitizenId == citizenId
                        join c in _dbContext.Categories.AsNoTracking() on r.CategoryId equals c.Id
                        select new { Request = r, CategoryName = c.Name };

        if (statusFilter.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Request.Status == statusFilter.Value);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.Request.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ServiceRequestSummaryDto(
                x.Request.Id,
                x.Request.Title,
                x.Request.CategoryId,
                x.CategoryName,
                x.Request.Status,
                x.Request.Priority,
                x.Request.Location != null
                    ? new LocationDto(x.Request.Location.Latitude, x.Request.Location.Longitude, x.Request.Location.AddressText)
                    : null,
                x.Request.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceRequestSummaryDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<MunicipalServiceRequestSummaryDto>> GetMunicipalSummariesAsync(
        RequestStatus? statusFilter,
        Guid? categoryId,
        Guid? departmentId,
        Priority? priority,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = from r in _dbContext.ServiceRequests.AsNoTracking()
                        join c in _dbContext.Categories.AsNoTracking() on r.CategoryId equals c.Id
                        join citizen in _dbContext.Users.AsNoTracking() on r.CitizenId equals citizen.Id
                        from dept in _dbContext.Departments.AsNoTracking().Where(d => d.Id == r.AssignedDepartmentId).DefaultIfEmpty()
                        from emp in _dbContext.Users.AsNoTracking().Where(u => u.Id == r.AssignedEmployeeId).DefaultIfEmpty()
                        select new
                        {
                            Request = r,
                            CategoryName = c.Name,
                            CitizenFirstName = citizen.Name.FirstName,
                            CitizenLastName = citizen.Name.LastName,
                            DepartmentName = dept != null ? dept.Name : null,
                            EmployeeFirstName = emp != null ? emp.Name.FirstName : null,
                            EmployeeLastName = emp != null ? emp.Name.LastName : null
                        };

        if (statusFilter.HasValue)
            baseQuery = baseQuery.Where(x => x.Request.Status == statusFilter.Value);

        if (categoryId.HasValue)
            baseQuery = baseQuery.Where(x => x.Request.CategoryId == categoryId.Value);

        if (departmentId.HasValue)
            baseQuery = baseQuery.Where(x => x.Request.AssignedDepartmentId == departmentId.Value);

        if (priority.HasValue)
            baseQuery = baseQuery.Where(x => x.Request.Priority == priority.Value);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.Request.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MunicipalServiceRequestSummaryDto(
                x.Request.Id,
                x.Request.Title,
                x.Request.CategoryId,
                x.CategoryName,
                x.Request.CitizenId,
                x.CitizenFirstName + " " + x.CitizenLastName,
                x.Request.Status,
                x.Request.Priority,
                x.Request.AssignedDepartmentId,
                x.DepartmentName,
                x.Request.AssignedEmployeeId,
                x.EmployeeFirstName != null ? x.EmployeeFirstName + " " + x.EmployeeLastName : null,
                x.Request.Location != null
                    ? new LocationDto(x.Request.Location.Latitude, x.Request.Location.Longitude, x.Request.Location.AddressText)
                    : null,
                x.Request.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<MunicipalServiceRequestSummaryDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<EmployeeServiceRequestSummaryDto>> GetSummariesByAssignedEmployeeIdAsync(
        Guid employeeId,
        RequestStatus? statusFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = from r in _dbContext.ServiceRequests.AsNoTracking()
                        where r.AssignedEmployeeId == employeeId
                        join c in _dbContext.Categories.AsNoTracking() on r.CategoryId equals c.Id
                        select new { Request = r, CategoryName = c.Name };

        if (statusFilter.HasValue)
            baseQuery = baseQuery.Where(x => x.Request.Status == statusFilter.Value);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.Request.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EmployeeServiceRequestSummaryDto(
                x.Request.Id,
                x.Request.Title,
                x.Request.CategoryId,
                x.CategoryName,
                x.Request.Status,
                x.Request.Priority,
                x.Request.Location != null
                    ? new LocationDto(x.Request.Location.Latitude, x.Request.Location.Longitude, x.Request.Location.AddressText)
                    : null,
                x.Request.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployeeServiceRequestSummaryDto>(items, totalCount, pageNumber, pageSize);
    }
}
