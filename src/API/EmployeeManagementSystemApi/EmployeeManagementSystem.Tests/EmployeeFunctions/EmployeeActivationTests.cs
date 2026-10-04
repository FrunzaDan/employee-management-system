using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeActivationTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private const string PerformedBy = "TestEmployer";

    [Fact]
    public async Task DeactivateEmployeeAsync_DelegatesToTheDbLayerWithTheGivenEmployeeId_AndLogsAnAuditEntry()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee deactivated successfully.");
        dbUtils.Setup(d => d.DeactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var activation = new EmployeeActivation(dbUtils.Object, auditLogger.Object);

        var result = await activation.DeactivateEmployeeAsync(EmployeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.DeactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
        dbUtils.Verify(d => d.ReactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        auditLogger.Verify(a => a.LogAsync(EmployeeId, PerformedBy, AuditAction.Deactivated, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_DoesNotLogAnAuditEntry_WhenTheDbLayerRejectsIt()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(409, "Employee is already deactivated.");
        dbUtils.Setup(d => d.DeactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var activation = new EmployeeActivation(dbUtils.Object, auditLogger.Object);

        var result = await activation.DeactivateEmployeeAsync(EmployeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        auditLogger.Verify(a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var activation = new EmployeeActivation(dbUtils.Object, auditLogger.Object);

        var result = await activation.DeactivateEmployeeAsync(Guid.Empty, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.DeactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReactivateEmployeeAsync_DelegatesToTheDbLayerWithTheGivenEmployeeId_AndLogsAnAuditEntry()
    {
        var dbUtils = new Mock<IDbUtils>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee reactivated successfully.");
        dbUtils.Setup(d => d.ReactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var activation = new EmployeeActivation(dbUtils.Object, auditLogger.Object);

        var result = await activation.ReactivateEmployeeAsync(EmployeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.ReactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
        dbUtils.Verify(d => d.DeactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        auditLogger.Verify(a => a.LogAsync(EmployeeId, PerformedBy, AuditAction.Reactivated, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
