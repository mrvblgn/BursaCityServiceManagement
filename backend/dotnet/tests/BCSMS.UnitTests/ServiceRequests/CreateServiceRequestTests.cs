using BCSMS.Application.Common.Exceptions;
using BCSMS.Application.ServiceRequests;
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;
using BCSMS.UnitTests.Fakes;
using Xunit;

namespace BCSMS.UnitTests.ServiceRequests;

public class CreateServiceRequestTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeServiceRequestRepository _requestRepository = new();
    private readonly FakeClock _clock = new();
    private readonly ServiceRequestService _service;

    public CreateServiceRequestTests()
    {
        _service = new ServiceRequestService(
            _requestRepository,
            _userRepository,
            _categoryRepository,
            _clock);
    }

    [Fact]
    public async Task CreateAsync_WithValidCitizenAndActiveCategory_ShouldSucceedAndReturnResponse()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var citizen = new User(
            citizenId,
            new FullName("Ali", "Yilmaz"),
            new ContactInfo("ali@bursa.bel.tr"),
            UserRole.Citizen,
            departmentId: null,
            _clock.UtcNow);
        _userRepository.Add(citizen);

        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "Road Damage", "Potholes and road issues", _clock.UtcNow);
        _categoryRepository.Add(category);

        var command = new CreateServiceRequestCommand(
            citizenId,
            "Pothole on Main Street",
            categoryId,
            "Large pothole near central station",
            40.1885,
            29.0610,
            "Ataturk Caddesi No: 15");

        // Act
        var response = await _service.CreateAsync(command);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(command.Title, response.Title);
        Assert.Equal(categoryId, response.CategoryId);
        Assert.Equal(citizenId, response.CitizenId);
        Assert.Equal(RequestStatus.New, response.Status);
        Assert.Equal(_clock.UtcNow, response.CreatedAt);

        Assert.Single(_requestRepository.Requests);
        var created = _requestRepository.Requests[0];
        Assert.Equal(command.Title, created.Title);
        Assert.NotNull(created.Location);
        Assert.Equal(40.1885, created.Location.Latitude);
        Assert.Equal(29.0610, created.Location.Longitude);
    }

    [Fact]
    public async Task CreateAsync_WithNonexistentCitizen_ShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "Cleaning", null, _clock.UtcNow);
        _categoryRepository.Add(category);

        var command = new CreateServiceRequestCommand(
            Guid.NewGuid(),
            "Trash overflow",
            categoryId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_WithInactiveCitizen_ShouldThrowApplicationConflictException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var citizen = new User(
            citizenId,
            new FullName("Fatma", "Demir"),
            new ContactInfo("fatma@bursa.bel.tr"),
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        citizen.Deactivate(_clock.UtcNow);
        _userRepository.Add(citizen);

        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "Cleaning", null, _clock.UtcNow);
        _categoryRepository.Add(category);

        var command = new CreateServiceRequestCommand(
            citizenId,
            "Trash overflow",
            categoryId);

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationConflictException>(() => _service.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_WithNonCitizenRole_ShouldThrowForbiddenException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var employee = new User(
            employeeId,
            new FullName("Mehmet", "Oz"),
            new ContactInfo("mehmet@bursa.bel.tr"),
            UserRole.Employee,
            deptId,
            _clock.UtcNow);
        _userRepository.Add(employee);

        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "Cleaning", null, _clock.UtcNow);
        _categoryRepository.Add(category);

        var command = new CreateServiceRequestCommand(
            employeeId,
            "Broken lamp",
            categoryId);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_WithNonexistentCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var citizen = new User(
            citizenId,
            new FullName("Ayse", "Kaya"),
            new ContactInfo("ayse@bursa.bel.tr"),
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        _userRepository.Add(citizen);

        var command = new CreateServiceRequestCommand(
            citizenId,
            "Broken streetlight",
            Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_WithInactiveCategory_ShouldThrowApplicationConflictException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var citizen = new User(
            citizenId,
            new FullName("Ayse", "Kaya"),
            new ContactInfo("ayse@bursa.bel.tr"),
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        _userRepository.Add(citizen);

        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, "Old Category", null, _clock.UtcNow);
        category.Deactivate(_clock.UtcNow);
        _categoryRepository.Add(category);

        var command = new CreateServiceRequestCommand(
            citizenId,
            "Streetlight issue",
            categoryId);

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationConflictException>(() => _service.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_WithLatitudeWithoutLongitude_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateServiceRequestCommand(
            Guid.NewGuid(),
            "Title",
            Guid.NewGuid(),
            Latitude: 40.1885,
            Longitude: null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_WithLongitudeWithoutLatitude_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateServiceRequestCommand(
            Guid.NewGuid(),
            "Title",
            Guid.NewGuid(),
            Latitude: null,
            Longitude: 29.0610);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(command));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateServiceRequestCommand(
            Guid.NewGuid(),
            "   ",
            Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(command));
    }
}
