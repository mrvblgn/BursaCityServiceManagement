using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.ServiceRequests;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;
using BCSMS.UnitTests.Fakes;
using Xunit;

namespace BCSMS.UnitTests.ServiceRequests;

public class GetServiceRequestByIdTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeServiceRequestRepository _requestRepository;
    private readonly FakeClock _clock = new();
    private readonly ServiceRequestService _service;

    public GetServiceRequestByIdTests()
    {
        _requestRepository = new FakeServiceRequestRepository(_categoryRepository);
        _service = new ServiceRequestService(
            _requestRepository,
            _userRepository,
            _categoryRepository,
            _clock);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnerCitizenRequests_ShouldReturnDetailedDto()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "Lighting", "Lighting issues", _clock.UtcNow);
        _categoryRepository.Add(category);

        var request = new ServiceRequest(
            Guid.NewGuid(),
            "Dark Street",
            categoryId,
            citizenId,
            _clock.UtcNow,
            "No street light on Elm st",
            new Location(40.1885, 29.0610, "Elm St"));

        request.StartReview(Guid.NewGuid(), _clock.UtcNow);
        request.AddComment("Staff assigned to review", citizenId, _clock.UtcNow);
        request.AddAttachment("photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg", citizenId, _clock.UtcNow);

        _requestRepository.Requests.Add(request);

        var query = new GetServiceRequestByIdQuery(request.Id, citizenId);

        // Act
        var result = await _service.GetByIdAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Id, result.Id);
        Assert.Equal("Dark Street", result.Title);
        Assert.Equal("Lighting", result.CategoryName);
        Assert.Equal(RequestStatus.Reviewing, result.Status);
        Assert.NotNull(result.Location);
        Assert.Equal("Elm St", result.Location.AddressText);
        Assert.Single(result.StatusHistory);
        Assert.Single(result.Comments);
        Assert.Single(result.Attachments);
        Assert.Equal("photo.jpg", result.Attachments[0].FileName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDifferentUserRequests_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerCitizenId = Guid.NewGuid();
        var otherCitizenId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var request = new ServiceRequest(
            Guid.NewGuid(),
            "Noise Complaint",
            categoryId,
            ownerCitizenId,
            _clock.UtcNow);
        _requestRepository.Requests.Add(request);

        var query = new GetServiceRequestByIdQuery(request.Id, otherCitizenId);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetByIdAsync(query));
    }

    [Fact]
    public async Task GetByIdAsync_WhenRequestDoesNotExist_ShouldThrowNotFoundException()
    {
        var query = new GetServiceRequestByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(query));
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyRequestId_ShouldThrowValidationException()
    {
        var query = new GetServiceRequestByIdQuery(Guid.Empty, Guid.NewGuid());
        await Assert.ThrowsAsync<ValidationException>(() => _service.GetByIdAsync(query));
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyRequestingUserId_ShouldThrowValidationException()
    {
        var query = new GetServiceRequestByIdQuery(Guid.NewGuid(), Guid.Empty);
        await Assert.ThrowsAsync<ValidationException>(() => _service.GetByIdAsync(query));
    }
}
