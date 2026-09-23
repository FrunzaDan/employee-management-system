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
        // Employee_Create's address parameters have no SQL-side defaults, so without this
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
    [InlineData(3)]
    [InlineData(255)]
    public async Task RegisterEmployeeFunction_RejectsAnUndefinedGenderCode_WithoutTouchingTheDb(byte gender)
    {
        // Malformed dates no longer reach this layer (DateOnly model binding rejects them), but
        // JSON-to-enum binding accepts any integer — Gender is the check that's still ours.
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
            Gender = (Gender)gender,
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Gender", result.ResponseMessage);
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
        Assert.Equal(EmployeeStatus.Active, capturedRequest!.EmployeeStatus);
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
            EmployeeStatus = EmployeeStatus.Test,
        };

        var result = await registration.RegisterEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.Equal(EmployeeStatus.Test, capturedRequest!.EmployeeStatus);
    }

    [Theory]
    [InlineData(1903)]
    [InlineData(1)]
    public async Task RegisterEmployeeFunction_RejectsAnyOtherStatus_WithoutTouchingTheDb(short status)
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var registration = new EmployeeRegistration(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel
        {
            Email = "dan@example.com",
            Msisdn = "123456789",
            Address = new AddressModel { Country = "Romania" },
            EmployeeStatus = (EmployeeStatus)status,
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
        var clientSuppliedGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
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
        Assert.NotEqual(Guid.Empty, capturedRequest.Guid);
        auditLogger.Verify(
            a => a.Log(capturedRequest.Guid!.Value, EmployerId, "Created", "Email: dan@example.com, MSISDN: 123456789",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
