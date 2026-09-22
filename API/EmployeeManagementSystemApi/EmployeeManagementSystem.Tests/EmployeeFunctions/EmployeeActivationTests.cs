using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeActivationTests
{
    private const string Guid = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
    private const string EmployerId = "TestEmployerID";

    [Fact]
    public async Task DeactivateEmployee_DelegatesToTheDbLayerWithTheGivenGuid_AndLogsAnAuditEntry()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee deactivated successfully.");
        dbUtils.Setup(d => d.DeactivateEmployee(Guid, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var activation = new EmployeeActivation(dbUtils.Object, auditLogger.Object);

        var result = await activation.DeactivateEmployee(Guid, EmployerId, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.DeactivateEmployee(Guid, It.IsAny<CancellationToken>()), Times.Once);
        dbUtils.Verify(d => d.ReactivateEmployee(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        auditLogger.Verify(a => a.Log(Guid, EmployerId, "Deactivated", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateEmployee_DoesNotLogAnAuditEntry_WhenTheDbLayerRejectsIt()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(409, "Employee already deactivated or update failed.");
        dbUtils.Setup(d => d.DeactivateEmployee(Guid, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var activation = new EmployeeActivation(dbUtils.Object, auditLogger.Object);

        var result = await activation.DeactivateEmployee(Guid, EmployerId, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        auditLogger.Verify(a => a.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ReactivateEmployee_DelegatesToTheDbLayerWithTheGivenGuid_AndLogsAnAuditEntry()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee reactivated successfully.");
        dbUtils.Setup(d => d.ReactivateEmployee(Guid, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var activation = new EmployeeActivation(dbUtils.Object, auditLogger.Object);

        var result = await activation.ReactivateEmployee(Guid, EmployerId, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.ReactivateEmployee(Guid, It.IsAny<CancellationToken>()), Times.Once);
        dbUtils.Verify(d => d.DeactivateEmployee(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        auditLogger.Verify(a => a.Log(Guid, EmployerId, "Reactivated", null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
