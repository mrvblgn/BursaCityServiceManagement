using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BCSMS.API.Contracts.Auth;
using BCSMS.API.Contracts.Manager;
using BCSMS.API.Contracts.ServiceRequests;
using BCSMS.Application.Auth.Login;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.Manager.GetMunicipal;
using BCSMS.Domain.Enums;
using BCSMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BCSMS.IntegrationTests.Manager;

public class ManagerEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Guid FenIsleriDeptId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid ParkDeptId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid RoadCategoryId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid Employee1Id = Guid.Parse("30000000-0000-0000-0000-000000000002"); // Fen İşleri
    private static readonly Guid Employee2Id = Guid.Parse("30000000-0000-0000-0000-000000000003"); // Park ve Bahçeler

    public ManagerEndpointsTests(CustomWebApplicationFactory factory)
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

    private async Task<(Guid RequestId, string CitizenToken)> CreateCitizenRequestAsync(string title = "Pothole on Main Road")
    {
        var citizenEmail = $"citizen_{Guid.NewGuid():N}@bursa.bel.tr";
        var reg = new RegisterRequest("Vatandas", "Test", citizenEmail, null, "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", reg);

        var token = await LoginAsync(citizenEmail, "Password123!");

        var createReq = new CreateServiceRequestApiRequest(title, RoadCategoryId, "Road damage");
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(createReq)
        };
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(msg);
        var created = await response.Content.ReadFromJsonAsync<CreateServiceRequestResponse>(_jsonOptions);

        return (created!.Id, token);
    }

    [Fact]
    public async Task GetMunicipalRequests_WithManagerJwt_ShouldReturn200()
    {
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/manager/service-requests?pageNumber=1&pageSize=10");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<MunicipalServiceRequestSummaryDto>>(_jsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetMunicipalRequests_WithCitizenJwt_ShouldReturn403Forbidden()
    {
        var (_, citizenToken) = await CreateCitizenRequestAsync();

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/manager/service-requests");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizenToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StartReview_WithManagerJwt_ShouldReturn204AndTransitionStatus()
    {
        var (requestId, _) = await CreateCitizenRequestAsync();
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        var msg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/review");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify status is Reviewing
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/manager/service-requests/{requestId}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var getResponse = await _client.SendAsync(getMsg);
        var detail = await getResponse.Content.ReadFromJsonAsync<ServiceRequestDetailDto>(_jsonOptions);

        Assert.NotNull(detail);
        Assert.Equal(RequestStatus.Reviewing, detail.Status);
    }

    [Fact]
    public async Task AssignRequest_WithValidEmployee_ShouldReturn204AndSetAssignedStatus()
    {
        var (requestId, _) = await CreateCitizenRequestAsync();
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        // Review first
        var reviewMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/review");
        reviewMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        await _client.SendAsync(reviewMsg);

        // Assign to Employee1 (Fen İşleri)
        var assignReq = new AssignRequestApiRequest(FenIsleriDeptId, Employee1Id, Priority.High);
        var assignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/assign")
        {
            Content = JsonContent.Create(assignReq)
        };
        assignMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var response = await _client.SendAsync(assignMsg);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify detail
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/manager/service-requests/{requestId}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var getResponse = await _client.SendAsync(getMsg);
        var detail = await getResponse.Content.ReadFromJsonAsync<ServiceRequestDetailDto>(_jsonOptions);

        Assert.NotNull(detail);
        Assert.Equal(RequestStatus.Assigned, detail.Status);
        Assert.Equal(FenIsleriDeptId, detail.AssignedDepartmentId);
        Assert.Equal(Employee1Id, detail.AssignedEmployeeId);
        Assert.Equal(Priority.High, detail.Priority);
    }

    [Fact]
    public async Task AssignRequest_WithMismatchedDepartment_ShouldReturn409Conflict()
    {
        var (requestId, _) = await CreateCitizenRequestAsync();
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        // Review
        var reviewMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/review");
        reviewMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        await _client.SendAsync(reviewMsg);

        // Assign Employee1 (belongs to Fen İşleri) to Park ve Bahçeler
        var assignReq = new AssignRequestApiRequest(ParkDeptId, Employee1Id, Priority.Medium);
        var assignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/assign")
        {
            Content = JsonContent.Create(assignReq)
        };
        assignMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var response = await _client.SendAsync(assignMsg);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task RejectRequest_WithManagerJwt_ShouldReturn204()
    {
        var (requestId, _) = await CreateCitizenRequestAsync();
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        var rejectMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/reject")
        {
            Content = JsonContent.Create(new WorkflowNoteApiRequest("Private property"))
        };
        rejectMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var response = await _client.SendAsync(rejectMsg);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify detail status
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/manager/service-requests/{requestId}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var getResponse = await _client.SendAsync(getMsg);
        var detail = await getResponse.Content.ReadFromJsonAsync<ServiceRequestDetailDto>(_jsonOptions);

        Assert.NotNull(detail);
        Assert.Equal(RequestStatus.Rejected, detail.Status);
    }
}
