using BCSMS.Application.Auth;
using BCSMS.Application.Auth.Login;
using BCSMS.Application.Auth.Register;
using BCSMS.Application.Common.Exceptions;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;
using BCSMS.UnitTests.Fakes;
using Xunit;

namespace BCSMS.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator;
    private readonly FakeClock _clock = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _jwtTokenGenerator = new FakeJwtTokenGenerator(_clock);

        _service = new AuthService(
            _userRepository,
            _passwordHasher,
            _jwtTokenGenerator,
            _clock);
    }

    [Fact]
    public async Task RegisterAsync_WithValidCitizenData_ShouldSucceedAndReturnResponse()
    {
        // Arrange
        var command = new RegisterCommand("Zeynep", "Aydin", "zeynep@bursa.bel.tr", "5551112233", "Password123!");

        // Act
        var response = await _service.RegisterAsync(command);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Zeynep", response.FirstName);
        Assert.Equal("Aydin", response.LastName);
        Assert.Equal("zeynep@bursa.bel.tr", response.Email);
        Assert.Equal(UserRole.Citizen, response.Role);
        Assert.Equal(_clock.UtcNow, response.CreatedAt);

        var savedUser = await _userRepository.GetByIdAsync(response.Id);
        Assert.NotNull(savedUser);
        Assert.True(_passwordHasher.VerifyPassword("Password123!", savedUser.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowApplicationConflictException()
    {
        // Arrange
        var existingUser = new User(
            Guid.NewGuid(),
            new FullName("Zeynep", "Aydin"),
            new ContactInfo("zeynep@bursa.bel.tr"),
            "hashed_pw",
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        _userRepository.Add(existingUser);

        var command = new RegisterCommand("Zeynep", "Aydin", "ZEYNEP@BURSA.BEL.TR", null, "Password123!");

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationConflictException>(() => _service.RegisterAsync(command));
    }

    [Fact]
    public async Task RegisterAsync_WithShortPassword_ShouldThrowValidationException()
    {
        var command = new RegisterCommand("Ali", "Can", "ali@bursa.bel.tr", null, "123");
        await Assert.ThrowsAsync<ValidationException>(() => _service.RegisterAsync(command));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenAndUserDto()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var passwordHash = _passwordHasher.HashPassword(password);
        var user = new User(
            Guid.NewGuid(),
            new FullName("Murat", "Koc"),
            new ContactInfo("murat@bursa.bel.tr"),
            passwordHash,
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        _userRepository.Add(user);

        var command = new LoginCommand("murat@bursa.bel.tr", password);

        // Act
        var response = await _service.LoginAsync(command);

        // Assert
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.Equal(_clock.UtcNow.AddMinutes(60), response.ExpiresAt);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal("Murat", response.User.FirstName);
        Assert.Equal("murat@bursa.bel.tr", response.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithNonexistentEmail_ShouldThrowUnauthorizedException()
    {
        var command = new LoginCommand("unknown@bursa.bel.tr", "SomePassword!");
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(command));
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var passwordHash = _passwordHasher.HashPassword("ActualPassword123!");
        var user = new User(
            Guid.NewGuid(),
            new FullName("Murat", "Koc"),
            new ContactInfo("murat@bursa.bel.tr"),
            passwordHash,
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        _userRepository.Add(user);

        var command = new LoginCommand("murat@bursa.bel.tr", "WrongPassword!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(command));
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var passwordHash = _passwordHasher.HashPassword("Password123!");
        var user = new User(
            Guid.NewGuid(),
            new FullName("Murat", "Koc"),
            new ContactInfo("murat@bursa.bel.tr"),
            passwordHash,
            UserRole.Citizen,
            null,
            _clock.UtcNow);
        user.Deactivate(_clock.UtcNow);
        _userRepository.Add(user);

        var command = new LoginCommand("murat@bursa.bel.tr", "Password123!");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(command));
        Assert.Equal("Invalid email or password.", ex.Message);
    }
}
