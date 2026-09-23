using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeUpdatingTests
{
    private static readonly Guid ValidEmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private const string PerformedBy = "TestEmployer";

    [Fact]
    public async Task UpdateEmployeeFunction_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        // A malformed GUID is already rejected by model binding; Guid.Empty is what a missing
        // one binds to.
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeUpdating(dbUtils.Object, auditLogger.Object);
        var request = new UpdateEmployeeRequest { EmployeeId = Guid.Empty };

        var result = await editing.UpdateEmployeeFunction(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("employee ID", result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateEmployee(It.IsAny<UpdateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeFunction_RejectsInvalidEmail_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeUpdating(dbUtils.Object, auditLogger.Object);
        var request = new UpdateEmployeeRequest { EmployeeId = ValidEmployeeId, Email = "not-an-email" };

        var result = await editing.UpdateEmployeeFunction(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Email", result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateEmployee(It.IsAny<UpdateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeFunction_RejectsInvalidPhoneNumber_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeUpdating(dbUtils.Object, auditLogger.Object);
        var request = new UpdateEmployeeRequest { EmployeeId = ValidEmployeeId, PhoneNumber = "123" };

        var result = await editing.UpdateEmployeeFunction(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("phone number", result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateEmployee(It.IsAny<UpdateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeFunction_RejectsAnUndefinedGender_WithoutTouchingTheDb()
    {
        // A JSON number binds to the enum even when it isn't one of its members.
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeUpdating(dbUtils.Object, auditLogger.Object);
        var request = new UpdateEmployeeRequest { EmployeeId = ValidEmployeeId, Gender = (Gender)3 };

        var result = await editing.UpdateEmployeeFunction(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Gender", result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateEmployee(It.IsAny<UpdateEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeFunction_AllowsOmittedEmailAndPhoneNumber()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        dbUtils.Setup(d => d.UpdateEmployee(It.IsAny<UpdateEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Employee updated successfully."));
        var editing = new EmployeeUpdating(dbUtils.Object, auditLogger.Object);
        var request = new UpdateEmployeeRequest { EmployeeId = ValidEmployeeId, FirstName = "Dan" };

        var result = await editing.UpdateEmployeeFunction(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        dbUtils.Verify(d => d.UpdateEmployee(request, It.IsAny<CancellationToken>()), Times.Once);
        auditLogger.Verify(a => a.Log(ValidEmployeeId, PerformedBy, AuditAction.Edited, "Updated: first name", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeFunction_PassesTheRequestThroughToTheDb_WhenAllFieldsAreValid()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        dbUtils.Setup(d => d.UpdateEmployee(It.IsAny<UpdateEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Employee updated successfully."));
        var editing = new EmployeeUpdating(dbUtils.Object, auditLogger.Object);
        var request = new UpdateEmployeeRequest
        {
            EmployeeId = ValidEmployeeId, Email = "dan@example.com", PhoneNumber = "123456789", BirthDate = new DateOnly(1990, 1, 2)
        };

        var result = await editing.UpdateEmployeeFunction(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        dbUtils.Verify(d => d.UpdateEmployee(request, It.IsAny<CancellationToken>()), Times.Once);
        auditLogger.Verify(a => a.Log(ValidEmployeeId, PerformedBy, AuditAction.Edited, "Updated: email, phone number, birth date", It.IsAny<CancellationToken>()), Times.Once);
    }
}
