using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.BusinessLogic.Configuration;
using EmployeeManagementSystem.BusinessLogic.Services.Implementation;
using EmployeeManagementSystem.Domain.Models;
using EmployeeManagementSystem.Tests.AuthFunctions;
using Microsoft.Extensions.Options;
using Moq;

namespace EmployeeManagementSystem.Tests.Services;

public class AuthServiceTests
{
    private static IOptions<AuthOptions> CreateOptions() => Options.Create(new AuthOptions
    {
        SecureJwtKey = "UGxlYXNlIHN0b3JlIHRoaXMgc2VjdXJpdHkga2V5IGluIGEgc2VjdXJlIGVudmlyb25tZW50IQ==",
        JwtIssuer = "https://localhost:7145/",
        JwtAudience = "https://localhost:7145/",
        AccessTokenTimeoutMinutes = 15
    });

    private static AuthService CreateSut(Mock<IDbUtils> dbUtils) =>
        new(new JwtCreation(CreateOptions(), dbUtils.Object));

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsAToken_WhenCredentialsAreValid()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.SetupEmployer("Employer123");
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessTokenAsync(new EmployerCredentials
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
    public async Task GetAccessTokenAsync_RejectsMissingUsername_WithoutTouchingTheDb(string? username)
    {
        var dbUtils = new Mock<IDbUtils>();
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessTokenAsync(new EmployerCredentials
        {
            Username = username,
            Password = "Employer123",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(
            d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAccessTokenAsync_RejectsMissingPassword_WithoutTouchingTheDb(string? password)
    {
        var dbUtils = new Mock<IDbUtils>();
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessTokenAsync(new EmployerCredentials
        {
            Username = "TestEmployer",
            Password = password,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Username and password are required.", result.ResponseMessage);
        dbUtils.Verify(
            d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsUnauthorized_WhenThePasswordIsWrong()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.SetupEmployer("Employer123");
        var sut = CreateSut(dbUtils);

        var result = await sut.GetAccessTokenAsync(new EmployerCredentials
        {
            Username = "TestEmployer",
            Password = "WrongPassword",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(401, result.Status);
        Assert.Equal("Invalid username or password.", result.ResponseMessage);
    }
}
