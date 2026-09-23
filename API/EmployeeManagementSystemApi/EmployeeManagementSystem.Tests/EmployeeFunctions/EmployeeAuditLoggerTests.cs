using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeAuditLoggerTests
{
    private static readonly Guid EmployeeGuid = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private const string EmployerId = "TestEmployerID";

    [Fact]
    public async Task Log_PassesTheGivenArgumentsThroughToTheDbLayer()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAudit(EmployeeGuid, EmployerId, "Edited", "Updated: email", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Success!"));
        var logger = new Mock<ILogger<EmployeeAuditLogger>>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger.Object);

        await auditLogger.Log(EmployeeGuid, EmployerId, "Edited", "Updated: email", TestContext.Current.CancellationToken);

        dbUtils.Verify(
            d => d.LogEmployeeAudit(EmployeeGuid, EmployerId, "Edited", "Updated: email", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Log_DefaultsDetailsToNull_WhenNotProvided()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAudit(EmployeeGuid, EmployerId, "Deactivated", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Success!"));
        var logger = new Mock<ILogger<EmployeeAuditLogger>>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger.Object);

        await auditLogger.Log(EmployeeGuid, EmployerId, "Deactivated", cancellationToken: TestContext.Current.CancellationToken);

        dbUtils.Verify(
            d => d.LogEmployeeAudit(EmployeeGuid, EmployerId, "Deactivated", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Writing an audit entry is best-effort and always runs after the employee mutation
    // it's recording has already succeeded (see EmployeeAuditLogger's own comment) — a DB
    // hiccup here must never surface as an exception to the caller. Every other test file
    // mocks IEmployeeAuditLogger away, so this is the only place that actually exercises
    // that swallow-and-log behavior against the real class.
    [Fact]
    public async Task Log_SwallowsAnyExceptionFromTheDbLayer_InsteadOfPropagatingIt()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAudit(EmployeeGuid, EmployerId, "Created", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection string is unreachable."));
        var logger = new Mock<ILogger<EmployeeAuditLogger>>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger.Object);

        var exception = await Record.ExceptionAsync(() => auditLogger.Log(EmployeeGuid, EmployerId, "Created", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Log_LogsAnError_WhenTheDbLayerThrows()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAudit(EmployeeGuid, EmployerId, "Created", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection string is unreachable."));
        var logger = new Mock<ILogger<EmployeeAuditLogger>>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger.Object);

        await auditLogger.Log(EmployeeGuid, EmployerId, "Created", cancellationToken: TestContext.Current.CancellationToken);

        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is InvalidOperationException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
