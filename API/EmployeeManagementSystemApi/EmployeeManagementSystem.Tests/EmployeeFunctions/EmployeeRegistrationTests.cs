using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeRegistrationTests
{
    private const string EmployerId = "TestEmployerID";

    [Theory]
    [InlineData(null, "Frunza")]
    [InlineData("", "Frunza")]
    [InlineData("Dan", null)]
    [InlineData("Dan", "")]
    public async Task RegisterEmployeeFunction_RejectsMissingName_WithoutTouchingTheDb(string? firstName,
        string? lastName)
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel
        {
            FirstName = firstName, LastName = lastName, Email = "dan@example.com", Msisdn = "123456789"
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("name", result.ResponseMessage, StringComparison.OrdinalIgnoreCase);
        dbUtils.Verify(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task RegisterEmployeeFunction_RejectsInvalidEmail_WithoutTouchingTheDb(string? email)
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { FirstName = "Dan", LastName = "Frunza", Email = email, Msisdn = "123456789" };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Email", result.ResponseMessage);
        dbUtils.Verify(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public async Task RegisterEmployeeFunction_RejectsInvalidMsisdn_WithoutTouchingTheDb(string? msisdn)
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { FirstName = "Dan", LastName = "Frunza", Email = "dan@example.com", Msisdn = msisdn };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("MSISDN", result.ResponseMessage);
        dbUtils.Verify(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterEmployeeFunction_RejectsAMissingAddress_WithoutTouchingTheDb()
    {
        // usp_createEmployee's address parameters have no SQL-side defaults, so without this
        // check a missing Address would otherwise surface as an opaque 500 instead of a 400.
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel
        {
            FirstName = "Dan", LastName = "Frunza", Email = "dan@example.com", Msisdn = "123456789", Address = null
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Address", result.ResponseMessage);
        dbUtils.Verify(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData("2020-13-40")]
    public async Task RegisterEmployeeFunction_RejectsInvalidHireDate_WithoutTouchingTheDb(string hireDate)
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel
        {
            FirstName = "Dan",
            LastName = "Frunza",
            Email = "dan@example.com",
            Msisdn = "123456789",
            Address = new AddressModel { Country = "Romania" },
            HireDate = hireDate,
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Hire date", result.ResponseMessage);
        dbUtils.Verify(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterEmployeeFunction_DefaultsEmployeeStatusToActive_WhenNoneIsSupplied()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        EmployeeModel? capturedRequest = null;
        dbUtils.Setup(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()))
            .Callback<EmployeeModel, CancellationToken>((c, _) => capturedRequest = c)
            .ReturnsAsync(new ResponseModel<object>(200, "Employee created successfully."));
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel
        {
            FirstName = "Dan",
            LastName = "Frunza",
            Email = "dan@example.com",
            Msisdn = "123456789",
            Address = new AddressModel { Country = "Romania" },
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.Equal(EmployeeStatusCodes.Active, capturedRequest!.EmployeeStatus);
    }

    [Fact]
    public async Task RegisterEmployeeFunction_AllowsExplicitlyRequestingTheTestStatus()
    {
        // Used by the About page's "add 50 test employees" bulk generator.
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        EmployeeModel? capturedRequest = null;
        dbUtils.Setup(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()))
            .Callback<EmployeeModel, CancellationToken>((c, _) => capturedRequest = c)
            .ReturnsAsync(new ResponseModel<object>(200, "Employee created successfully."));
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel
        {
            FirstName = "Dan",
            LastName = "Frunza",
            Email = "dan@example.com",
            Msisdn = "123456789",
            Address = new AddressModel { Country = "Romania" },
            EmployeeStatus = EmployeeStatusCodes.Test,
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.Equal(EmployeeStatusCodes.Test, capturedRequest!.EmployeeStatus);
    }

    [Theory]
    [InlineData(1903)]
    [InlineData(1)]
    public async Task RegisterEmployeeFunction_RejectsAnyOtherStatus_WithoutTouchingTheDb(int status)
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel
        {
            Email = "dan@example.com",
            Msisdn = "123456789",
            Address = new AddressModel { Country = "Romania" },
            EmployeeStatus = status,
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterEmployeeFunction_AlwaysGeneratesAFreshServerSideGuid_IgnoringAnyClientSuppliedValue()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        EmployeeModel? capturedRequest = null;
        dbUtils.Setup(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()))
            .Callback<EmployeeModel, CancellationToken>((c, _) => capturedRequest = c)
            .ReturnsAsync(new ResponseModel<object>(200, "Employee created successfully."));
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        const string clientSuppliedGuid = "11111111-1111-1111-1111-111111111111";
        var request = new EmployeeModel
        {
            Guid = clientSuppliedGuid,
            FirstName = "Dan",
            LastName = "Frunza",
            Email = "dan@example.com",
            Msisdn = "123456789",
            Address = new AddressModel { Country = "Romania" },
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        dbUtils.Verify(d => d.RegisterEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(capturedRequest!.Guid);
        Assert.NotEqual(clientSuppliedGuid, capturedRequest.Guid);
        Assert.True(Guid.TryParse(capturedRequest.Guid, out _));
        auditLogger.Verify(
            a => a.Log(capturedRequest.Guid!, EmployerId, "Created", "Email: dan@example.com, MSISDN: 123456789",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
