using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeDeletionTests
{
    private const string EmployerId = "TestEmployerID";

    [Fact]
    public async Task DeleteEmployee_DelegatesToTheDbLayerWithTheGivenGuid_AndLogsAnAuditEntry()
    {
        var guid = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee deleted successfully.");
        dbUtils.Setup(d => d.DeleteEmployee(guid, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var deletion = new EmployeeDeletion(dbUtils.Object, auditLogger.Object);

        var result = await deletion.DeleteEmployee(guid, EmployerId, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.DeleteEmployee(guid, It.IsAny<CancellationToken>()), Times.Once);
        auditLogger.Verify(a => a.Log(guid, EmployerId, "Deleted", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployee_PropagatesABusinessRuleRejection_WithoutModifyingIt()
    {
        // Mirrors the real usp_deleteEmployee rule: an active employee can't be deleted directly.
        var guid = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(409, "Employee must be deactivated before it can be deleted.");
        dbUtils.Setup(d => d.DeleteEmployee(guid, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var deletion = new EmployeeDeletion(dbUtils.Object, auditLogger.Object);

        var result = await deletion.DeleteEmployee(guid, EmployerId, TestContext.Current.CancellationToken);

        Assert.Equal(409, result.Status);
        Assert.Equal(expected.ResponseMessage, result.ResponseMessage);
        auditLogger.Verify(a => a.Log(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
