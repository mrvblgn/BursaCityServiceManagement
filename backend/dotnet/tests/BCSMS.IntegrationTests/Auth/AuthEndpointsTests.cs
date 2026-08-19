using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BCSMS.API.Contracts.Auth;
using BCSMS.Application.Auth.Login;
using BCSMS.Application.Auth.Register;
using BCSMS.Domain.Enums;
using BCSMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BCSMS.IntegrationTests.Auth;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidCitizenData_ShouldReturn201Created()
    {
        // Arrange
        var request = new RegisterRequest(
            "Ahmet",
            "Yildiz",
            $"ahmet_{Guid.NewGuid():N}@bursa.bel.tr",
            "5551234567",
            "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Ahmet", result.FirstName);
        Assert.Equal("Yildiz", result.LastName);
        Assert.Equal(request.Email.ToLowerInvariant(), result.Email);
        Assert.Equal(UserRole.Citizen, result.Role);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409Conflict()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid():N}@bursa.bel.tr";
        var request = new RegisterRequest("Mehmet", "Kaya", email, null, "Password123!");

        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        var problem = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200OkWithJwt()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid():N}@bursa.bel.tr";
        var password = "SecurePassword123!";
        var registerRequest = new RegisterRequest("Emre", "Demir", email, null, password);

        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        var loginRequest = new LoginRequest(email, password);

        // Act
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(email.ToLowerInvariant(), result.User.Email);
        Assert.Equal(UserRole.Citizen, result.User.Role);
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_ShouldReturn401Unauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@bursa.bel.tr", "SomePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(401, problem.Status);
        Assert.Equal("Invalid email or password.", problem.Detail);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401Unauthorized()
    {
        // Arrange
        var email = $"wrongpw_{Guid.NewGuid():N}@bursa.bel.tr";
        var registerRequest = new RegisterRequest("Can", "Tekin", email, null, "CorrectPassword123!");

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(email, "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(401, problem.Status);
        Assert.Equal("Invalid email or password.", problem.Detail);
    }
}
