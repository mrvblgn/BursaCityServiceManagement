using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BCSMS.API.Contracts.Auth;
using BCSMS.Application.Auth.Login;
using BCSMS.Application.Reference;
using BCSMS.IntegrationTests.Infrastructure;
using Xunit;

namespace BCSMS.IntegrationTests.Reference;

public class ReferenceDataEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Guid FenIsleriDeptId = Guid.Parse("10000000-0000-0000-0000-000000000001");

    public ReferenceDataEndpointsTests(CustomWebApplicationFactory factory)
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
    public async Task GetCategories_WithAuthenticatedCitizen_ShouldReturn200WithActiveCategories()
    {
        var citizenEmail = $"citizen_{Guid.NewGuid():N}@bursa.bel.tr";
        var reg = new RegisterRequest("Citizen", "User", citizenEmail, null, "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", reg);
        var citizenToken = await LoginAsync(citizenEmail, "Password123!");

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/categories");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizenToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryLookupDto>>(_jsonOptions);
        Assert.NotNull(categories);
        Assert.True(categories.Count >= 4);
        Assert.Contains(categories, c => c.Name == "Yol ve Kaldırım");
    }

    [Fact]
    public async Task GetDepartments_WithManagerJwt_ShouldReturn200WithActiveDepartments()
    {
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/departments");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentLookupDto>>(_jsonOptions);
        Assert.NotNull(departments);
        Assert.True(departments.Count >= 4);
        Assert.Contains(departments, d => d.Name == "Fen İşleri");
    }

    [Fact]
    public async Task GetDepartmentEmployees_WithManagerJwt_ShouldReturn200WithEmployeesInDepartment()
    {
        var managerToken = await LoginAsync("manager@bursa.bel.tr");

        var msg = new HttpRequestMessage(HttpMethod.Get, $"/api/departments/{FenIsleriDeptId}/employees");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeLookupDto>>(_jsonOptions);
        Assert.NotNull(employees);
        Assert.NotEmpty(employees);
        Assert.Contains(employees, e => e.Email == "employee1@bursa.bel.tr");
    }

    [Fact]
    public async Task GetDepartments_WithCitizenJwt_ShouldReturn403Forbidden()
    {
        var citizenEmail = $"citizen_{Guid.NewGuid():N}@bursa.bel.tr";
        var reg = new RegisterRequest("Citizen", "User", citizenEmail, null, "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", reg);
        var citizenToken = await LoginAsync(citizenEmail, "Password123!");

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/departments");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", citizenToken);
        var response = await _client.SendAsync(msg);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
