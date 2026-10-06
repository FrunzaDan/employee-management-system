using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class DeleteEmployeeHandlerTests
{
    private const string PerformedBy = "TestEmployer";

    [Fact]
    public async Task DeleteEmployeeAsync_DelegatesToTheDbLayerWithTheGivenEmployeeId_AndLogsAnAuditEntry()
    {
        var employeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee deleted successfully.");
        employees.Setup(d => d.DeleteEmployeeAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new DeleteEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(employeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        employees.Verify(d => d.DeleteEmployeeAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
        auditLogger.Verify(a => a.LogAsync(employeeId, PerformedBy, AuditAction.Deleted, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_PropagatesABusinessRuleRejection_WithoutModifyingIt()
    {
        var employeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(409, "Employee must be deactivated before it can be deleted.");
        employees.Setup(d => d.DeleteEmployeeAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new DeleteEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(employeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(409, result.Status);
        Assert.Equal(expected.ResponseMessage, result.ResponseMessage);
        auditLogger.Verify(a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var handler = new DeleteEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(Guid.Empty, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.DeleteEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        auditLogger.Verify(a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
