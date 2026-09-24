using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.BusinessLogic.Services.Implementation;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Configuration;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Services;

public class AuthServiceTests
{
    private static Mock<IAppSettingsConfig> CreateConfig()
    {
        var config = new Mock<IAppSettingsConfig>();
        config.Setup(c => c.SecureJwtKey)
            .Returns("UGxlYXNlIHN0b3JlIHRoaXMgc2VjdXJpdHkga2V5IGluIGEgc2VjdXJlIGVudmlyb25tZW50IQ==");
        config.Setup(c => c.JwtIssuer).Returns("https://localhost:7145/");
        config.Setup(c => c.JwtAudience).Returns("https://localhost:7145/");
        config.Setup(c => c.AccessTokenTimeout).Returns("15");
        return config;
    }

    private static AuthService CreateSut(Mock<IDbUtils> dbUtils) =>
        new(new JwtCreation(CreateConfig().Object, dbUtils.Object));

    [Fact]
    public async Task GetAccessToken_ReturnsAToken_WhenCredentialsAreValid()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<EmployerRole?>(200, "Success!", EmployerRole.Employer));
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            Username = "TestEmployer",
            Password = "Employer123",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.IsType<AccessTokenResponse>(result.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAccessToken_RejectsMissingUsername_WithoutTouchingTheDb(string? username)
    {
        var dbUtils = new Mock<IDbUtils>();
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            Username = username,
            Password = "Employer123",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(
            d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Distinct from JwtCreation's own guard, which only checks Username — AuthService is
    // the one place that also rejects a missing/blank password before any DB call is made.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAccessToken_RejectsMissingPassword_WithoutTouchingTheDb(string? password)
    {
        var dbUtils = new Mock<IDbUtils>();
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            Username = "TestEmployer",
            Password = password,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Username and password are required.", result.ResponseMessage);
        dbUtils.Verify(
            d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccessToken_PropagatesTheDbRejection_WhenCredentialsAreWrong()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<EmployerRole?>(401, "Invalid username or password."));
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            Username = "TestEmployer",
            Password = "WrongPassword",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(401, result.Status);
        Assert.Equal("Invalid username or password.", result.ResponseMessage);
    }
}
