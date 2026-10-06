using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Configuration;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Auth;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Auth;

public class JwtCreationTests
{
    private static IOptions<AuthOptions> CreateOptions() => Options.Create(new AuthOptions
    {
        SecureJwtKey = "UGxlYXNlIHN0b3JlIHRoaXMgc2VjdXJpdHkga2V5IGluIGEgc2VjdXJlIGVudmlyb25tZW50IQ==",
        JwtIssuer = "https://localhost:7145/",
        JwtAudience = "https://localhost:7145/",
        AccessTokenTimeoutMinutes = 15
    });

    private static EmployerCredentials Credentials => new()
    {
        Username = "TestEmployer",
        Password = "Employer123",
    };

    [Fact]
    public async Task GenerateBearerJwtAsync_ReturnsAToken_WhenCredentialsAreValid()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.SetupEmployer("Employer123");
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);

        var result = await jwtCreation.GenerateBearerJwtAsync(Credentials, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        var data = Assert.IsType<AccessTokenResponse>(result.Data);
        Assert.Equal(DateTimeKind.Utc, data.ExpiresAt.Kind);
        Assert.False(string.IsNullOrWhiteSpace(data.AccessToken));
        Assert.True(data.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateBearerJwtAsync_WritesIatAsANumericDate_AndTheRoleAsItsNumericCode()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.SetupEmployer("Employer123");
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);

        var result = await jwtCreation.GenerateBearerJwtAsync(Credentials, TestContext.Current.CancellationToken);
        var token = new JsonWebTokenHandler().ReadJsonWebToken(result.Data!.AccessToken);

        Assert.IsType<long>(token.GetPayloadValue<object>(JwtRegisteredClaimNames.Iat));
        Assert.Contains(token.Claims, c => c.Type == "role" && c.Value == "1801");
    }

    [Fact]
    public async Task GenerateBearerJwtAsync_ReturnsUnauthorized_WhenThePasswordIsWrong()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.SetupEmployer("SomeOtherPassword");
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);

        var result = await jwtCreation.GenerateBearerJwtAsync(Credentials, TestContext.Current.CancellationToken);

        Assert.Equal(401, result.Status);
        Assert.Equal("Invalid username or password.", result.ResponseMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateBearerJwtAsync_ReturnsBadRequest_WithoutTouchingTheDb_WhenUsernameIsMissing(string? username)
    {
        var employers = new Mock<IEmployerRepository>();
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);
        var credentials = new EmployerCredentials { Username = username, Password = "Employer123" };

        var result = await jwtCreation.GenerateBearerJwtAsync(credentials, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employers.Verify(d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateBearerJwtAsync_LetsADbFailurePropagate_ToTheGlobalExceptionHandler()
    {
        var employers = new Mock<IEmployerRepository>();
        var failure = new InvalidOperationException("The database is unreachable.");
        employers.Setup(d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            jwtCreation.GenerateBearerJwtAsync(Credentials, TestContext.Current.CancellationToken));
        Assert.Same(failure, exception);
    }

    [Fact]
    public async Task GenerateBearerJwtAsync_ReturnsUnauthorized_WhenTheUsernameIsUnknown()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.Setup(d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployerAuthData?)null);
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);

        var result = await jwtCreation.GenerateBearerJwtAsync(Credentials, TestContext.Current.CancellationToken);

        Assert.Equal(401, result.Status);
        Assert.Equal("Invalid username or password.", result.ResponseMessage);
        employers.Verify(d => d.RecordEmployerLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateBearerJwtAsync_ReturnsForbidden_WhenTheRoleIsNotEmployer()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.SetupEmployer("Employer123", role: (EmployerRole)1802);
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);

        var result = await jwtCreation.GenerateBearerJwtAsync(Credentials, TestContext.Current.CancellationToken);

        Assert.Equal(403, result.Status);
        Assert.Equal("The provided employer role (1802) is not valid.", result.ResponseMessage);
        employers.Verify(d => d.RecordEmployerLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateBearerJwtAsync_RecordsTheLogin_OnlyWhenItSucceeds()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.SetupEmployer("Employer123");
        var jwtCreation = new JwtCreation(CreateOptions(), employers.Object);

        await jwtCreation.GenerateBearerJwtAsync(Credentials, TestContext.Current.CancellationToken);

        employers.Verify(d => d.RecordEmployerLoginAsync("TestEmployer", It.IsAny<CancellationToken>()), Times.Once);
    }
}
