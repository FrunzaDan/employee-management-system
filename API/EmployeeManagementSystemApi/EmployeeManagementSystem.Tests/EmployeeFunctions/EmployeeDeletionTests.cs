using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeDeletionTests
{
    private const string PerformedBy = "TestEmployer";

    [Fact]
    public async Task DeleteEmployeeAsync_DelegatesToTheDbLayerWithTheGivenEmployeeId_AndLogsAnAuditEntry()
    {
        var employeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee deleted successfully.");
        dbUtils.Setup(d => d.DeleteEmployeeAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var deletion = new EmployeeDeletion(dbUtils.Object, auditLogger.Object);

        var result = await deletion.DeleteEmployeeAsync(employeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.DeleteEmployeeAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
        auditLogger.Verify(a => a.LogAsync(employeeId, PerformedBy, AuditAction.Deleted, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_PropagatesABusinessRuleRejection_WithoutModifyingIt()
    {
        // Mirrors the real Employee_Delete rule: an active employee can't be deleted directly.
        var employeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(409, "Employee must be deactivated before it can be deleted.");
        dbUtils.Setup(d => d.DeleteEmployeeAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var deletion = new EmployeeDeletion(dbUtils.Object, auditLogger.Object);

        var result = await deletion.DeleteEmployeeAsync(employeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(409, result.Status);
        Assert.Equal(expected.ResponseMessage, result.ResponseMessage);
        auditLogger.Verify(a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
