using System.Net;
using EmployeeManagementSystem.Domain.Models;
using EmployeeManagementSystem.Tests.AuthFunctions;
using Moq;

namespace EmployeeManagementSystem.Tests.Endpoints;

public class AuthenticationEndpointTests
{
    [Fact]
    public async Task PostAccessToken_IssuesATokenThatTheApiThenAccepts()
    {
        await using var api = new ApiHost(signedIn: false);
        api.Db.SetupEmployer("secret");

        var login = await api.PostAsync("/api/authentication/access-token",
            new { username = "employer", password = "secret" });

        var token = (await ApiHost.ReadEnvelopeAsync(login)).GetProperty("data").GetProperty("accessToken").GetString();
        api.Client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        await ApiHost.ReadEnvelopeAsync(await api.GetAsync("/api/authentication/verify-token"));
    }

    [Fact]
    public async Task PostAccessToken_WrongCredentials_IsA401Problem()
    {
        await using var api = new ApiHost(signedIn: false);
        api.Db.SetupEmployer("secret");

        var response = await api.PostAsync("/api/authentication/access-token",
            new { username = "employer", password = "wrong" });

        var problem = await ApiHost.ReadProblemAsync(response, HttpStatusCode.Unauthorized);
        Assert.Equal("Invalid username or password.", problem.GetProperty("detail").GetString());
    }
}
