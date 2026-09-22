using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeEditingTests
{
    private const string ValidGuid = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
    private const string EmployerId = "TestEmployerID";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    public async Task EditEmployeeFunction_RejectsInvalidGuid_WithoutTouchingTheDb(string? guid)
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeEditing(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { Guid = guid };

        var result = await editing.EditEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Guid", result.ResponseMessage);
        dbUtils.Verify(d => d.EditEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditEmployeeFunction_RejectsInvalidEmail_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeEditing(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { Guid = ValidGuid, Email = "not-an-email" };

        var result = await editing.EditEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Email", result.ResponseMessage);
        dbUtils.Verify(d => d.EditEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditEmployeeFunction_RejectsInvalidMsisdn_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeEditing(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { Guid = ValidGuid, Msisdn = "123" };

        var result = await editing.EditEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("MSISDN", result.ResponseMessage);
        dbUtils.Verify(d => d.EditEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditEmployeeFunction_RejectsInvalidHireDate_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var editing = new EmployeeEditing(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { Guid = ValidGuid, HireDate = "not-a-date" };

        var result = await editing.EditEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("Hire date", result.ResponseMessage);
        dbUtils.Verify(d => d.EditEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditEmployeeFunction_AllowsOmittedEmailAndMsisdn()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        dbUtils.Setup(d => d.EditEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Employee updated successfully."));
        var editing = new EmployeeEditing(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { Guid = ValidGuid, FirstName = "Dan" };

        var result = await editing.EditEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        dbUtils.Verify(d => d.EditEmployee(request, It.IsAny<CancellationToken>()), Times.Once);
        auditLogger.Verify(a => a.Log(ValidGuid, EmployerId, "Edited", "Updated: first name", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditEmployeeFunction_PassesTheRequestThroughToTheDb_WhenAllFieldsAreValid()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        dbUtils.Setup(d => d.EditEmployee(It.IsAny<EmployeeModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Employee updated successfully."));
        var editing = new EmployeeEditing(dbUtils.Object, auditLogger.Object);
        var request = new EmployeeModel { Guid = ValidGuid, Email = "dan@example.com", Msisdn = "123456789" };

        var result = await editing.EditEmployeeFunction(request, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        dbUtils.Verify(d => d.EditEmployee(request, It.IsAny<CancellationToken>()), Times.Once);
        auditLogger.Verify(a => a.Log(ValidGuid, EmployerId, "Edited", "Updated: email, MSISDN", It.IsAny<CancellationToken>()), Times.Once);
    }
}
