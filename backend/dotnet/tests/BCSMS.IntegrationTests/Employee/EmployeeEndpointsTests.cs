using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BCSMS.API.Contracts.Auth;
using BCSMS.API.Contracts.Employee;
using BCSMS.API.Contracts.Manager;
using BCSMS.API.Contracts.ServiceRequests;
using BCSMS.Application.Auth.Login;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Application.ServiceRequests.Employee.GetAssigned;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Domain.Enums;
using BCSMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BCSMS.IntegrationTests.Employee;

public class EmployeeEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Guid FenIsleriDeptId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid RoadCategoryId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid Employee1Id = Guid.Parse("30000000-0000-0000-0000-000000000002"); // Fen İşleri

    public EmployeeEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsync(string email, string password = "Demo12345!")
    {
        var loginRequest = new LoginRequest(email, password);
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
        return result!.AccessToken;
    }

    private async Task<Guid> CreateAndAssignRequestToEmployee1Async()
    {
        var citizenEmail = $"citizen_{Guid.NewGuid():N}@bursa.bel.tr";
        var reg = new RegisterRequest("Citizen", "Demo", citizenEmail, null, "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", reg);

        var citizenToken = await LoginAsync(citizenEmail, "Password123!");
        var createReq = new CreateServiceRequestApiRequest("Pothole repair", RoadCategoryId);
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(createReq)
        };
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizenToken);
        var createResp = await _client.SendAsync(msg);
        var created = await createResp.Content.ReadFromJsonAsync<CreateServiceRequestResponse>(_jsonOptions);

        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        // Review
        var reviewMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{created!.Id}/review");
        reviewMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        await _client.SendAsync(reviewMsg);

        // Assign to Employee1
        var assignReq = new AssignRequestApiRequest(FenIsleriDeptId, Employee1Id, Priority.High);
        var assignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{created.Id}/assign")
        {
            Content = JsonContent.Create(assignReq)
        };
        assignMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        await _client.SendAsync(assignMsg);

        return created.Id;
    }

    [Fact]
    public async Task GetMyAssignedRequests_WithEmployeeJwt_ShouldReturn200WithAssignedRequests()
    {
        var requestId = await CreateAndAssignRequestToEmployee1Async();
        var emp1Token = await LoginAsync("employee1@bursa.bel.tr");

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/employee/service-requests");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp1Token);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeServiceRequestSummaryDto>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Contains(result.Items, r => r.Id == requestId);
    }

    [Fact]
    public async Task GetMyAssignedRequests_WithCitizenJwt_ShouldReturn403Forbidden()
    {
        var citizenEmail = $"citizen_{Guid.NewGuid():N}@bursa.bel.tr";
        var reg = new RegisterRequest("Citizen", "User", citizenEmail, null, "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", reg);
        var citizenToken = await LoginAsync(citizenEmail, "Password123!");

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/employee/service-requests");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizenToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAssignedRequestById_WhenAssignedToEmployee_ShouldReturn200()
    {
        var requestId = await CreateAndAssignRequestToEmployee1Async();
        var emp1Token = await LoginAsync("employee1@bursa.bel.tr");

        var msg = new HttpRequestMessage(HttpMethod.Get, $"/api/employee/service-requests/{requestId}");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp1Token);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<ServiceRequestDetailDto>(_jsonOptions);
        Assert.NotNull(detail);
        Assert.Equal(requestId, detail.Id);
        Assert.Equal(Employee1Id, detail.AssignedEmployeeId);
    }

    [Fact]
    public async Task GetAssignedRequestById_WhenAssignedToDifferentEmployee_ShouldReturn403Forbidden()
    {
        var requestId = await CreateAndAssignRequestToEmployee1Async();
        var emp2Token = await LoginAsync("employee2@bursa.bel.tr"); // employee2 is Park ve Bahçeler

        var msg = new HttpRequestMessage(HttpMethod.Get, $"/api/employee/service-requests/{requestId}");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp2Token);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(403, problem.Status);
    }

    [Fact]
    public async Task StartWorkAndResolve_WhenAssignedToEmployee_ShouldSucceed()
    {
        var requestId = await CreateAndAssignRequestToEmployee1Async();
        var emp1Token = await LoginAsync("employee1@bursa.bel.tr");

        // Start Work
        var startMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/employee/service-requests/{requestId}/start");
        startMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp1Token);
        var startResp = await _client.SendAsync(startMsg);
        Assert.Equal(HttpStatusCode.NoContent, startResp.StatusCode);

        // Resolve
        var resolveReq = new ResolveRequestApiRequest("Asphalt laid and compacted.");
        var resolveMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/employee/service-requests/{requestId}/resolve")
        {
            Content = JsonContent.Create(resolveReq)
        };
        resolveMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp1Token);
        var resolveResp = await _client.SendAsync(resolveMsg);
        Assert.Equal(HttpStatusCode.NoContent, resolveResp.StatusCode);

        // Verify status is Resolved
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/employee/service-requests/{requestId}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp1Token);
        var getResp = await _client.SendAsync(getMsg);
        var detail = await getResp.Content.ReadFromJsonAsync<ServiceRequestDetailDto>(_jsonOptions);

        Assert.NotNull(detail);
        Assert.Equal(RequestStatus.Resolved, detail.Status);
    }
}
