using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class DeactivateEmployeeHandlerTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private const string PerformedBy = "TestEmployer";

    [Fact]
    public async Task DeactivateEmployeeAsync_DelegatesToTheDbLayerWithTheGivenEmployeeId_AndLogsAnAuditEntry()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee deactivated successfully.");
        employees.Setup(d => d.DeactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new DeactivateEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(EmployeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        employees.Verify(d => d.DeactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
        employees.Verify(d => d.ReactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        auditLogger.Verify(a => a.LogAsync(EmployeeId, PerformedBy, AuditAction.Deactivated, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_DoesNotLogAnAuditEntry_WhenTheDbLayerRejectsIt()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(409, "Employee is already deactivated.");
        employees.Setup(d => d.DeactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new DeactivateEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(EmployeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        auditLogger.Verify(a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new DeactivateEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(Guid.Empty, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.DeactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
