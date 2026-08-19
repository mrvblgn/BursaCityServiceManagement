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
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Domain.Enums;
using BCSMS.IntegrationTests.Infrastructure;
using Xunit;

namespace BCSMS.IntegrationTests.Workflow;

public class EndToEndMunicipalWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Guid FenIsleriDeptId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid RoadCategoryId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid ManagerId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid Employee1Id = Guid.Parse("30000000-0000-0000-0000-000000000002");

    public EndToEndMunicipalWorkflowTests(CustomWebApplicationFactory factory)
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

    [Fact]
    public async Task CompleteMunicipalLifecycle_ShouldExecuteFullSequenceAndRecordStatusHistory()
    {
        // 1. Citizen submits a new request
        var citizenEmail = $"citizen_{Guid.NewGuid():N}@bursa.bel.tr";
        var reg = new RegisterRequest("Ahmet", "Citizen", citizenEmail, null, "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", reg);
        var citizenToken = await LoginAsync(citizenEmail, "Password123!");

        var createReq = new CreateServiceRequestApiRequest(
            "Deep pothole on FSM Boulevard",
            RoadCategoryId,
            "Pothole causing traffic delay",
            40.2100,
            28.9800,
            "Fatih Sultan Mehmet Bulvari No: 100");

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/service-requests")
        {
            Content = JsonContent.Create(createReq)
        };
        createMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizenToken);
        var createResp = await _client.SendAsync(createMsg);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<CreateServiceRequestResponse>(_jsonOptions);
        var requestId = created!.Id;

        // 2. Manager logs in and begins Reviewing
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        var reviewMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/review");
        reviewMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var reviewResp = await _client.SendAsync(reviewMsg);
        Assert.Equal(HttpStatusCode.NoContent, reviewResp.StatusCode);

        // 3. Manager assigns to Fen İşleri department and Employee 1 with High priority
        var assignReq = new AssignRequestApiRequest(FenIsleriDeptId, Employee1Id, Priority.High);
        var assignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/assign")
        {
            Content = JsonContent.Create(assignReq)
        };
        assignMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var assignResp = await _client.SendAsync(assignMsg);
        Assert.Equal(HttpStatusCode.NoContent, assignResp.StatusCode);

        // 4. Employee 1 logs in and starts work
        var emp1Token = await LoginAsync("employee1@bursa.bel.tr");

        var startMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/employee/service-requests/{requestId}/start");
        startMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp1Token);
        var startResp = await _client.SendAsync(startMsg);
        Assert.Equal(HttpStatusCode.NoContent, startResp.StatusCode);

        // 5. Employee 1 completes work and resolves request
        var resolveReq = new ResolveRequestApiRequest("Asphalt patching completed and road opened to traffic.");
        var resolveMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/employee/service-requests/{requestId}/resolve")
        {
            Content = JsonContent.Create(resolveReq)
        };
        resolveMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", emp1Token);
        var resolveResp = await _client.SendAsync(resolveMsg);
        Assert.Equal(HttpStatusCode.NoContent, resolveResp.StatusCode);

        // 6. Manager confirms resolution and closes request
        var closeReq = new WorkflowNoteApiRequest("On-site inspection passed. Quality verified.");
        var closeMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/manager/service-requests/{requestId}/close")
        {
            Content = JsonContent.Create(closeReq)
        };
        closeMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var closeResp = await _client.SendAsync(closeMsg);
        Assert.Equal(HttpStatusCode.NoContent, closeResp.StatusCode);

        // 7. Manager retrieves final request details and inspects status history
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/manager/service-requests/{requestId}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var getResp = await _client.SendAsync(getMsg);
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var detail = await getResp.Content.ReadFromJsonAsync<ServiceRequestDetailDto>(_jsonOptions);

        Assert.NotNull(detail);
        Assert.Equal(RequestStatus.Closed, detail.Status);
        Assert.Equal(Priority.High, detail.Priority);
        Assert.Equal(FenIsleriDeptId, detail.AssignedDepartmentId);
        Assert.Equal(Employee1Id, detail.AssignedEmployeeId);

        // 8. Verify the complete 5-step StatusHistory sequence
        Assert.Equal(5, detail.StatusHistory.Count);

        // Entry 1: New -> Reviewing (Manager)
        Assert.Equal(RequestStatus.New, detail.StatusHistory[0].OldStatus);
        Assert.Equal(RequestStatus.Reviewing, detail.StatusHistory[0].NewStatus);
        Assert.Equal(ManagerId, detail.StatusHistory[0].ChangedByUserId);

        // Entry 2: Reviewing -> Assigned (Manager)
        Assert.Equal(RequestStatus.Reviewing, detail.StatusHistory[1].OldStatus);
        Assert.Equal(RequestStatus.Assigned, detail.StatusHistory[1].NewStatus);
        Assert.Equal(ManagerId, detail.StatusHistory[1].ChangedByUserId);

        // Entry 3: Assigned -> InProgress (Employee 1)
        Assert.Equal(RequestStatus.Assigned, detail.StatusHistory[2].OldStatus);
        Assert.Equal(RequestStatus.InProgress, detail.StatusHistory[2].NewStatus);
        Assert.Equal(Employee1Id, detail.StatusHistory[2].ChangedByUserId);

        // Entry 4: InProgress -> Resolved (Employee 1)
        Assert.Equal(RequestStatus.InProgress, detail.StatusHistory[3].OldStatus);
        Assert.Equal(RequestStatus.Resolved, detail.StatusHistory[3].NewStatus);
        Assert.Equal(Employee1Id, detail.StatusHistory[3].ChangedByUserId);
        Assert.Equal("Asphalt patching completed and road opened to traffic.", detail.StatusHistory[3].Note);

        // Entry 5: Resolved -> Closed (Manager)
        Assert.Equal(RequestStatus.Resolved, detail.StatusHistory[4].OldStatus);
        Assert.Equal(RequestStatus.Closed, detail.StatusHistory[4].NewStatus);
        Assert.Equal(ManagerId, detail.StatusHistory[4].ChangedByUserId);
        Assert.Equal("On-site inspection passed. Quality verified.", detail.StatusHistory[4].Note);

        // Verify chronological timestamps
        for (int i = 1; i < detail.StatusHistory.Count; i++)
        {
            Assert.True(detail.StatusHistory[i].ChangedAt >= detail.StatusHistory[i - 1].ChangedAt);
        }
    }
}
