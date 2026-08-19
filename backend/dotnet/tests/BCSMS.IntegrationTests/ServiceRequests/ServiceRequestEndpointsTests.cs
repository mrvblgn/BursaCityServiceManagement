using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BCSMS.API.Contracts.Auth;
using BCSMS.API.Contracts.ServiceRequests;
using BCSMS.Application.Auth.Login;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Infrastructure.Persistence;
using BCSMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BCSMS.IntegrationTests.ServiceRequests;

public class ServiceRequestEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ServiceRequestEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCitizenAsync(string email, string password = "Password123!")
    {
        var registerRequest = new RegisterRequest("Test", "Citizen", email, null, password);
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);

        return loginResult!.AccessToken;
    }

    private async Task<Guid> SeedCategoryAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BcsmsDbContext>();

        var category = new Category(Guid.NewGuid(), name, null, DateTime.UtcNow);
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        return category.Id;
    }

    [Fact]
    public async Task Create_WithoutToken_ShouldReturn401Unauthorized()
    {
        var request = new CreateServiceRequestApiRequest("No Auth", Guid.NewGuid());
        var response = await _client.PostAsJsonAsync("/api/service-requests", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidToken_ShouldReturn401Unauthorized()
    {
        var request = new CreateServiceRequestApiRequest("Bad Token", Guid.NewGuid());

        var message = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "malformed.jwt.token");

        var response = await _client.SendAsync(message);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAuthenticatedCitizen_ShouldSucceedAndReturn201WithCitizenIdFromJwt()
    {
        // Arrange
        var token = await RegisterAndLoginCitizenAsync($"citizen_{Guid.NewGuid():N}@bursa.bel.tr");
        var categoryId = await SeedCategoryAsync("Parks and Gardens");

        var request = new CreateServiceRequestApiRequest(
            "Broken Bench in City Park",
            categoryId,
            "The wooden bench is broken near the fountain",
            40.1885,
            29.0610,
            "Kulturpark No: 4");

        var message = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(message);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"New\"", json);

        var result = await response.Content.ReadFromJsonAsync<CreateServiceRequestResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(categoryId, result.CategoryId);
        Assert.Equal(RequestStatus.New, result.Status);
    }

    [Fact]
    public async Task GetMy_WithAuthenticatedCitizen_ShouldReturnCitizenRequests()
    {
        // Arrange
        var token = await RegisterAndLoginCitizenAsync($"myreq_{Guid.NewGuid():N}@bursa.bel.tr");
        var categoryId = await SeedCategoryAsync("Waste Management");

        var createRequest = new CreateServiceRequestApiRequest("Trash collection needed", categoryId);

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(createMsg);

        // Act
        var getMsg = new HttpRequestMessage(HttpMethod.Get, "/api/service-requests/my");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(getMsg);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ServiceRequestSummaryDto>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Contains(result.Items, r => r.Title == "Trash collection needed");
        Assert.Equal("Waste Management", result.Items[0].CategoryName);
    }

    [Fact]
    public async Task GetById_WhenOwnerCitizenRequests_ShouldReturn200WithFullDetail()
    {
        // Arrange
        var token = await RegisterAndLoginCitizenAsync($"owner_{Guid.NewGuid():N}@bursa.bel.tr");
        var categoryId = await SeedCategoryAsync("Road Infrastructure");

        var createRequest = new CreateServiceRequestApiRequest(
            "Pothole Repair",
            categoryId,
            "Deep pothole on corner",
            40.1820,
            29.0650,
            "Heykel Meydani");

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResponse = await _client.SendAsync(createMsg);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateServiceRequestResponse>(_jsonOptions);

        // Act
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/service-requests/{created!.Id}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(getMsg);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<ServiceRequestDetailDto>(_jsonOptions);
        Assert.NotNull(detail);
        Assert.Equal(created.Id, detail.Id);
        Assert.Equal("Pothole Repair", detail.Title);
        Assert.Equal("Road Infrastructure", detail.CategoryName);
        Assert.NotNull(detail.Location);
        Assert.Equal("Heykel Meydani", detail.Location.AddressText);
    }

    [Fact]
    public async Task GetById_WhenAnotherCitizenRequests_ShouldReturn403Forbidden()
    {
        // Arrange
        var citizen1Token = await RegisterAndLoginCitizenAsync($"citizen1_{Guid.NewGuid():N}@bursa.bel.tr");
        var citizen2Token = await RegisterAndLoginCitizenAsync($"citizen2_{Guid.NewGuid():N}@bursa.bel.tr");
        var categoryId = await SeedCategoryAsync("Street Lighting");

        var createRequest = new CreateServiceRequestApiRequest("Dark Alley", categoryId);

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizen1Token);
        var createResponse = await _client.SendAsync(createMsg);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateServiceRequestResponse>(_jsonOptions);

        // Act - Citizen 2 attempts to read Citizen 1's request
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/service-requests/{created!.Id}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizen2Token);
        var response = await _client.SendAsync(getMsg);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(403, problem.Status);
    }
}
