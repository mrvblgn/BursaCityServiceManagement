using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Common;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;

namespace BCSMS.UnitTests.Fakes;

public class FakeServiceRequestRepository : IServiceRequestRepository
{
    public readonly List<ServiceRequest> Requests = new();
    private readonly FakeCategoryRepository? _categoryRepository;

    public FakeServiceRequestRepository(FakeCategoryRepository? categoryRepository = null)
    {
        _categoryRepository = categoryRepository;
    }

    public Task AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        Requests.Add(serviceRequest);
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
}
