using System.Net;
using System.Security.Claims;
using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace EmployeeManagementSystem.Tests.Security;

public class EndpointAuthorizationTests
{
    private const string SigningKey = "test-signing-key-that-is-at-least-32-bytes-long";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string DeleteAllAuditLogUrl = "/api/employee/audit-log/all";

    // The only API endpoints a caller may reach without a token.
    private static readonly HashSet<string> AnonymousEndpoints = ["POST /api/authentication/access-token"];

    private static WebApplicationFactory<Program> CreateFactory(Mock<IEmployeeService>? employeeService = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Auth:SecureJwtKey", SigningKey);
            builder.UseSetting("Auth:JwtIssuer", Issuer);
            builder.UseSetting("Auth:JwtAudience", Audience);
            if (employeeService is not null)
                builder.ConfigureTestServices(services => services.AddScoped(_ => employeeService.Object));
        });

    private static string CreateToken(string role, string issuer = Issuer) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, "TestEmployer"),
                new Claim(JwtRegisteredClaimNames.UniqueName, "TestEmployer"),
                new Claim("role", role)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(JwtSigningKey.Create(SigningKey),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = issuer,
            Audience = Audience
        });

    // HTTPS from the start: following the HTTP-to-HTTPS redirect would drop the Authorization header.
    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    private static IEnumerable<string> ApiEndpoints(WebApplicationFactory<Program> factory) =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText?.StartsWith("api/", StringComparison.Ordinal) == true)
            .SelectMany(e => (e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [])
                .Select(method => $"{method} /{e.RoutePattern.RawText}"))
            .Distinct();

    private static HttpRequestMessage Request(string endpoint, string? token = null)
    {
        var parts = endpoint.Split(' ');
        var request = new HttpRequestMessage(new HttpMethod(parts[0]), parts[1]);
        if (token is not null)
            request.Headers.Authorization = new("Bearer", token);
        return request;
    }

    [Fact]
    public async Task EveryApiEndpoint_RejectsAnAnonymousCaller_ExceptTheLogin()
    {
        await using var factory = CreateFactory();
        var client = CreateClient(factory);
        var endpoints = ApiEndpoints(factory).ToList();
        Assert.NotEmpty(endpoints);
        Assert.Subset(endpoints.ToHashSet(), AnonymousEndpoints);

        var open = new List<string>();
        foreach (var endpoint in endpoints.Except(AnonymousEndpoints))
        {
            var response = await client.SendAsync(Request(endpoint), TestContext.Current.CancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                open.Add($"{endpoint} -> {(int)response.StatusCode}");
        }

        Assert.Empty(open);
    }

    [Fact]
    public async Task AToken_FromAnotherIssuer_IsRejected()
    {
        await using var factory = CreateFactory();

        var response = await CreateClient(factory).SendAsync(
            Request("GET /api/authentication/verify-token", CreateToken("1801", issuer: "someone-else")),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1802")]
    public async Task DeletingTheWholeAuditLog_IsForbidden_WithoutTheEmployerRole(string role)
    {
        var employeeService = new Mock<IEmployeeService>();
        await using var factory = CreateFactory(employeeService);

        var response = await CreateClient(factory).SendAsync(
            Request($"DELETE {DeleteAllAuditLogUrl}", CreateToken(role)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        employeeService.Verify(s => s.DeleteAllEmployeeAuditLogAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletingTheWholeAuditLog_IsAllowed_WithTheEmployerRole()
    {
        var employeeService = new Mock<IEmployeeService>();
        employeeService.Setup(s => s.DeleteAllEmployeeAuditLogAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Audit log cleared."));
        await using var factory = CreateFactory(employeeService);

        var response = await CreateClient(factory).SendAsync(
            Request($"DELETE {DeleteAllAuditLogUrl}", CreateToken(((short)EmployerRole.Employer).ToString())),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        employeeService.Verify(s => s.DeleteAllEmployeeAuditLogAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TheSignedInUsername_IsRecordedAsWhoMadeTheChange()
    {
        var employeeService = new Mock<IEmployeeService>();
        employeeService.Setup(s => s.DeleteEmployeeAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Employee deleted successfully."));
        await using var factory = CreateFactory(employeeService);
        var employeeId = Guid.NewGuid();

        await CreateClient(factory).SendAsync(
            Request($"DELETE /api/employee/delete?employeeId={employeeId}", CreateToken("1801")),
            TestContext.Current.CancellationToken);

        employeeService.Verify(s => s.DeleteEmployeeAsync(employeeId, "TestEmployer", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
