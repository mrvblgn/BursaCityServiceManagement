using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.ServiceRequests;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.UnitTests.Fakes;
using Xunit;

namespace BCSMS.UnitTests.ServiceRequests;

public class GetMyServiceRequestsTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeServiceRequestRepository _requestRepository;
    private readonly FakeClock _clock = new();
    private readonly ServiceRequestService _service;

    public GetMyServiceRequestsTests()
    {
        _requestRepository = new FakeServiceRequestRepository(_categoryRepository);
        _service = new ServiceRequestService(
            _requestRepository,
            _userRepository,
            _categoryRepository,
            _clock);
    }

    [Fact]
    public async Task GetMyRequestsAsync_ShouldReturnCitizenRequestsWithPagination()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var otherCitizenId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "Parks", "Park issues", _clock.UtcNow);
        _categoryRepository.Add(category);

        for (int i = 1; i <= 5; i++)
        {
            var req = new ServiceRequest(
                Guid.NewGuid(),
                $"Request {i}",
                categoryId,
                citizenId,
                _clock.UtcNow.AddMinutes(i));
            _requestRepository.Requests.Add(req);
        }

        // Another citizen request
        var otherReq = new ServiceRequest(
            Guid.NewGuid(),
            "Other Citizen Request",
            categoryId,
            otherCitizenId,
            _clock.UtcNow);
        _requestRepository.Requests.Add(otherReq);

        var query = new GetMyServiceRequestsQuery(citizenId, PageNumber: 1, PageSize: 3);

        // Act
        var result = await _service.GetMyRequestsAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.Equal("Request 5", result.Items[0].Title); // newest first
        Assert.Equal("Parks", result.Items[0].CategoryName);
    }

    [Fact]
    public async Task GetMyRequestsAsync_WithStatusFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var req1 = new ServiceRequest(Guid.NewGuid(), "New Request", categoryId, citizenId, _clock.UtcNow);
        var req2 = new ServiceRequest(Guid.NewGuid(), "In Progress Request", categoryId, citizenId, _clock.UtcNow);
        req2.StartReview(Guid.NewGuid(), _clock.UtcNow);
        req2.Assign(Guid.NewGuid(), null, Priority.High, Guid.NewGuid(), _clock.UtcNow);
        req2.StartProgress(Guid.NewGuid(), _clock.UtcNow);

        _requestRepository.Requests.Add(req1);
        _requestRepository.Requests.Add(req2);

        var query = new GetMyServiceRequestsQuery(citizenId, StatusFilter: RequestStatus.InProgress);

        // Act
        var result = await _service.GetMyRequestsAsync(query);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("In Progress Request", result.Items[0].Title);
        Assert.Equal(RequestStatus.InProgress, result.Items[0].Status);
    }

    [Fact]
    public async Task GetMyRequestsAsync_WithEmptyCitizenId_ShouldThrowValidationException()
    {
        var query = new GetMyServiceRequestsQuery(Guid.Empty);
        await Assert.ThrowsAsync<ValidationException>(() => _service.GetMyRequestsAsync(query));
    }

    [Fact]
    public async Task GetMyRequestsAsync_WithInvalidPageNumber_ShouldThrowValidationException()
    {
        var query = new GetMyServiceRequestsQuery(Guid.NewGuid(), PageNumber: 0);
        await Assert.ThrowsAsync<ValidationException>(() => _service.GetMyRequestsAsync(query));
    }

    [Fact]
    public async Task GetMyRequestsAsync_WithInvalidPageSize_ShouldThrowValidationException()
    {
        var query = new GetMyServiceRequestsQuery(Guid.NewGuid(), PageSize: 150);
        await Assert.ThrowsAsync<ValidationException>(() => _service.GetMyRequestsAsync(query));
    }
}
