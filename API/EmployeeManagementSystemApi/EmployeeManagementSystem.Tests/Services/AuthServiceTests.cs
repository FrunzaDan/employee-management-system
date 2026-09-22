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
        config.Setup(c => c.JwtIssuer).Returns("https://localhost:7146/");
        config.Setup(c => c.JwtAudience).Returns("https://localhost:7146/");
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
            .ReturnsAsync(new ResponseModel<int?>(200, "Success!", 1801));
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            EmployerId = "TestEmployerID",
            EmployerPassword = "Employer123",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.IsType<AccessTokenResponse>(result.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAccessToken_RejectsMissingEmployerId_WithoutTouchingTheDb(string? employerId)
    {
        var dbUtils = new Mock<IDbUtils>();
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            EmployerId = employerId,
            EmployerPassword = "Employer123",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(403, result.Status);
        dbUtils.Verify(
            d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Distinct from JwtCreation's own guard, which only checks EmployerId — AuthService is
    // the one place that also rejects a missing/blank password before any DB call is made.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAccessToken_RejectsMissingEmployerPassword_WithoutTouchingTheDb(string? employerPassword)
    {
        var dbUtils = new Mock<IDbUtils>();
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            EmployerId = "TestEmployerID",
            EmployerPassword = employerPassword,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(403, result.Status);
        Assert.Equal("Invalid or empty employer credentials.", result.ResponseMessage);
        dbUtils.Verify(
            d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccessToken_PropagatesTheDbRejection_WhenCredentialsAreWrong()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CheckEmployerCredentialsFromDb(It.IsAny<EmployerCredentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<int?>(403, "Invalid Employer ID or Password."));
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessToken(new EmployerCredentials
        {
            EmployerId = "TestEmployerID",
            EmployerPassword = "WrongPassword",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(403, result.Status);
        Assert.Equal("Invalid Employer ID or Password.", result.ResponseMessage);
    }
}
