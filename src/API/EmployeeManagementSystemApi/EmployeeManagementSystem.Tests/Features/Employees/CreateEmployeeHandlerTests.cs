using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class CreateEmployeeHandlerTests
{
    private const string PerformedBy = "TestEmployer";

    private static CreateEmployeeRequest ValidRequest() => new()
    {
        FirstName = "Dan",
        LastName = "Frunza",
        Email = "dan@example.com",
        PhoneNumber = "123456789",
        Address = new AddressRequest
        {
            Country = "Romania",
            County = "Cluj",
            City = "Cluj-Napoca",
            PostalCode = "400001",
            Street = "Main",
            StreetNumber = "1"
        },
    };

    [Theory]
    [InlineData(null, "Frunza")]
    [InlineData("", "Frunza")]
    [InlineData("Dan", null)]
    [InlineData("Dan", "")]
    public async Task CreateEmployeeAsync_RejectsMissingName_WithoutTouchingTheDb(string? firstName,
        string? lastName)
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.FirstName = firstName;
        request.LastName = lastName;

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("name", result.ResponseMessage, StringComparison.OrdinalIgnoreCase);
        employees.Verify(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task CreateEmployeeAsync_RejectsInvalidEmail_WithoutTouchingTheDb(string? email)
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.Email = email;

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Email", result.ResponseMessage);
        employees.Verify(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_RejectsAnEmailLongerThanTheColumn_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.Email = new string('a', 250) + "@x.ro";

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Email is too long.", result.ResponseMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public async Task CreateEmployeeAsync_RejectsInvalidPhoneNumber_WithoutTouchingTheDb(string? phoneNumber)
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.PhoneNumber = phoneNumber;

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("phone number", result.ResponseMessage);
        employees.Verify(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_RejectsAMissingAddress_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.Address = null;

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Address", result.ResponseMessage);
        employees.Verify(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_RejectsAnAddressWithAMissingField_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.Address!.City = " ";

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("City is required.", result.ResponseMessage);
        employees.Verify(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_RejectsAnUndefinedGender_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.Gender = (Gender)7;

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Gender", result.ResponseMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(EmployeeStatus.Active)]
    [InlineData(EmployeeStatus.Test)]
    public async Task CreateEmployeeAsync_AcceptsNoStatusActiveOrTest(EmployeeStatus? status)
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        employees.Setup(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Employee created successfully.", Guid.NewGuid()));
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.Status = status;

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
    }

    [Theory]
    [InlineData(EmployeeStatus.Deactivated)]
    [InlineData((EmployeeStatus)1)]
    public async Task CreateEmployeeAsync_RejectsAnyOtherStatus_WithoutTouchingTheDb(EmployeeStatus status)
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);
        var request = ValidRequest();
        request.Status = status;

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ReturnsTheDbGeneratedEmployeeId_AndAuditLogsAgainstIt()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var newGuid = Guid.Parse("1352433e-f36b-1410-86a6-008ef0c0e32e");
        employees.Setup(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Employee created successfully.", newGuid));
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(ValidRequest(), PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.Equal(newGuid, result.Data);
        auditLogger.Verify(
            a => a.LogAsync(newGuid, PerformedBy, AuditAction.Created, "Email: dan@example.com, Phone number: 123456789",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_DoesNotAuditLog_WhenTheDbRejectsIt()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        employees.Setup(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(409, "Email already exists."));
        var handler = new CreateEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(ValidRequest(), PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(409, result.Status);
        auditLogger.Verify(
            a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }
}
