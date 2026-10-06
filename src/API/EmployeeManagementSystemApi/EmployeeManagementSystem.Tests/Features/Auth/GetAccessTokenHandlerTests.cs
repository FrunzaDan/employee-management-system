using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Configuration;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Auth;
using Microsoft.Extensions.Options;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Auth;

public class GetAccessTokenHandlerTests
{
    private static IOptions<AuthOptions> CreateOptions() => Options.Create(new AuthOptions
    {
        SecureJwtKey = "UGxlYXNlIHN0b3JlIHRoaXMgc2VjdXJpdHkga2V5IGluIGEgc2VjdXJlIGVudmlyb25tZW50IQ==",
        JwtIssuer = "https://localhost:7145/",
        JwtAudience = "https://localhost:7145/",
        AccessTokenTimeoutMinutes = 15
    });

    private static GetAccessTokenHandler CreateSut(Mock<IEmployerRepository> employers) =>
        new(new JwtCreation(CreateOptions(), employers.Object));

    [Fact]
    public async Task HandleAsync_ReturnsAToken_WhenCredentialsAreValid()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.SetupEmployer("Employer123");
        var sut = CreateSut(employers);

        var result = await sut.HandleAsync(new EmployerCredentials
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
    public async Task HandleAsync_RejectsMissingUsername_WithoutTouchingTheDb(string? username)
    {
        var employers = new Mock<IEmployerRepository>();
        var sut = CreateSut(employers);

        var result = await sut.HandleAsync(new EmployerCredentials
        {
            Username = username,
            Password = "Employer123",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employers.Verify(
            d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_RejectsMissingPassword_WithoutTouchingTheDb(string? password)
    {
        var employers = new Mock<IEmployerRepository>();
        var sut = CreateSut(employers);

        var result = await sut.HandleAsync(new EmployerCredentials
        {
            Username = "TestEmployer",
            Password = password,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Username and password are required.", result.ResponseMessage);
        employers.Verify(
            d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUnauthorized_WhenThePasswordIsWrong()
    {
        var employers = new Mock<IEmployerRepository>();
        employers.SetupEmployer("Employer123");
        var sut = CreateSut(employers);

        var result = await sut.HandleAsync(new EmployerCredentials
        {
            Username = "TestEmployer",
            Password = "WrongPassword",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(401, result.Status);
        Assert.Equal("Invalid username or password.", result.ResponseMessage);
    }
}
