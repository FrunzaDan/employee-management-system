using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Configuration;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.AuthFunctions;

public class JwtCreationTests
{
    private static Mock<IAppSettingsConfig> CreateConfig(string accessTokenTimeout = "15")
    {
        var config = new Mock<IAppSettingsConfig>();
        config.Setup(c => c.SecureJwtKey)
            .Returns("UGxlYXNlIHN0b3JlIHRoaXMgc2VjdXJpdHkga2V5IGluIGEgc2VjdXJlIGVudmlyb25tZW50IQ==");
        config.Setup(c => c.JwtIssuer).Returns("https://localhost:7146/");
        config.Setup(c => c.JwtAudience).Returns("https://localhost:7146/");
        config.Setup(c => c.AccessTokenTimeout).Returns(accessTokenTimeout);
        return config;
    }

    private static EmployerCredentials Credentials => new()
    {
        EmployerId = "TestEmployerID",
        EmployerPassword = "Employer123",
    };

    [Fact]
    public async Task GenerateBearerJwt_ReturnsAToken_WhenCredentialsAreValid()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<int?>(200, "Success!", 1801));
        var jwtCreation = new JwtCreation(CreateConfig().Object, dbUtils.Object);

        var result = await jwtCreation.GenerateBearerJwt(Credentials, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        var data = Assert.IsType<AccessTokenResponse>(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(data.AccessToken));
        Assert.True(data.ValidUntil > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateBearerJwt_ReturnsForbidden_WhenCredentialsAreRejectedByTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<int?>(403, "Invalid employer credentials."));
        var jwtCreation = new JwtCreation(CreateConfig().Object, dbUtils.Object);

        var result = await jwtCreation.GenerateBearerJwt(Credentials, TestContext.Current.CancellationToken);

        Assert.Equal(403, result.Status);
        Assert.Equal("Invalid employer credentials.", result.ResponseMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateBearerJwt_ReturnsForbidden_WithoutTouchingTheDb_WhenEmployerIdIsMissing(string? employerId)
    {
        var dbUtils = new Mock<IDbUtils>();
        var jwtCreation = new JwtCreation(CreateConfig().Object, dbUtils.Object);
        var credentials = new EmployerCredentials { EmployerId = employerId, EmployerPassword = "Employer123" };

        var result = await jwtCreation.GenerateBearerJwt(credentials, TestContext.Current.CancellationToken);

        Assert.Equal(403, result.Status);
        dbUtils.Verify(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateBearerJwt_ReturnsServerError_WhenAccessTokenTimeoutIsNotConfiguredAsANumber()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<int?>(200, "Success!", 1801));
        var jwtCreation = new JwtCreation(CreateConfig(accessTokenTimeout: "not-a-number").Object, dbUtils.Object);

        var result = await jwtCreation.GenerateBearerJwt(Credentials, TestContext.Current.CancellationToken);

        // AccessTokenTimeout is validated via double.TryParse before any token is built/signed,
        // so a bad config value returns this dedicated message rather than falling through to
        // BuildTokenDescriptor()'s double.Parse and being caught by the generic catch block.
        Assert.Equal(500, result.Status);
        Assert.Equal("Invalid AccessTokenTimeout configuration.", result.ResponseMessage);
    }

    [Fact]
    public async Task GenerateBearerJwt_DoesNotLeakExceptionDetails_WhenTokenGenerationFails()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection string 'EmployeeManagementSystemDB_Docker' is unreachable."));
        var jwtCreation = new JwtCreation(CreateConfig().Object, dbUtils.Object);

        var result = await jwtCreation.GenerateBearerJwt(Credentials, TestContext.Current.CancellationToken);

        // This endpoint is unauthenticated, unlike the rest of the API, and any exception here
        // is caught locally rather than reaching the global exception handler's Details-only-in-
        // Development guard — so the raw exception message must never reach the response.
        Assert.Equal(500, result.Status);
        Assert.DoesNotContain("Connection string", result.ResponseMessage);
        Assert.DoesNotContain("unreachable", result.ResponseMessage);
    }
}
