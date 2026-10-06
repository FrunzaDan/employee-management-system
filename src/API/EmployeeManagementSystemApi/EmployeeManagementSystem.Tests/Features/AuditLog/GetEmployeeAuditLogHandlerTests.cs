using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.AuditLog;

public class GetEmployeeAuditLogHandlerTests
{
    [Fact]
    public async Task GetEmployeeAuditLogAsync_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        var auditLog = new Mock<IAuditLogRepository>();
        var handler = new GetEmployeeAuditLogHandler(auditLog.Object);

        var result = await handler.HandleAsync(Guid.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        auditLog.Verify(d => d.GetEmployeeAuditLogAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
