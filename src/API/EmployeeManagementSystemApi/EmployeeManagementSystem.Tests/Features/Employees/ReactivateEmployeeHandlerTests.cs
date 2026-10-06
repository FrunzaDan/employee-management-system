using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class ReactivateEmployeeHandlerTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private const string PerformedBy = "TestEmployer";

    [Fact]
    public async Task ReactivateEmployeeAsync_DelegatesToTheDbLayerWithTheGivenEmployeeId_AndLogsAnAuditEntry()
    {
        var employees = new Mock<IEmployeeRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Employee reactivated successfully.");
        employees.Setup(d => d.ReactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new ReactivateEmployeeHandler(employees.Object, auditLogger.Object);

        var result = await handler.HandleAsync(EmployeeId, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        employees.Verify(d => d.ReactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
        employees.Verify(d => d.DeactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        auditLogger.Verify(a => a.LogAsync(EmployeeId, PerformedBy, AuditAction.Reactivated, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
