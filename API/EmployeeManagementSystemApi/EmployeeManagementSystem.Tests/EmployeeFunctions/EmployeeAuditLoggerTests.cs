using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeAuditLoggerTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private const string PerformedBy = "TestEmployer";

    [Fact]
    public async Task LogAsync_PassesTheGivenArgumentsThroughToTheDbLayer()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAuditAsync(EmployeeId, PerformedBy, AuditAction.Edited, "Updated: email", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Success!"));
        var logger = new FakeLogger<EmployeeAuditLogger>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger);

        await auditLogger.LogAsync(EmployeeId, PerformedBy, AuditAction.Edited, "Updated: email", TestContext.Current.CancellationToken);

        dbUtils.Verify(
            d => d.LogEmployeeAuditAsync(EmployeeId, PerformedBy, AuditAction.Edited, "Updated: email", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_DefaultsDetailsToNull_WhenNotProvided()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAuditAsync(EmployeeId, PerformedBy, AuditAction.Deactivated, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Success!"));
        var logger = new FakeLogger<EmployeeAuditLogger>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger);

        await auditLogger.LogAsync(EmployeeId, PerformedBy, AuditAction.Deactivated, cancellationToken: TestContext.Current.CancellationToken);

        dbUtils.Verify(
            d => d.LogEmployeeAuditAsync(EmployeeId, PerformedBy, AuditAction.Deactivated, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_SwallowsAnyExceptionFromTheDbLayer_InsteadOfPropagatingIt()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAuditAsync(EmployeeId, PerformedBy, AuditAction.Created, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection string is unreachable."));
        var logger = new FakeLogger<EmployeeAuditLogger>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger);

        var exception = await Record.ExceptionAsync(() => auditLogger.LogAsync(EmployeeId, PerformedBy, AuditAction.Created, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LogAsync_LogsAnError_WhenTheDbLayerThrows()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.LogEmployeeAuditAsync(EmployeeId, PerformedBy, AuditAction.Created, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection string is unreachable."));
        var logger = new FakeLogger<EmployeeAuditLogger>();
        var auditLogger = new EmployeeAuditLogger(dbUtils.Object, logger);

        await auditLogger.LogAsync(EmployeeId, PerformedBy, AuditAction.Created, cancellationToken: TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal(2, entry.Id.Id);
        Assert.IsType<InvalidOperationException>(entry.Exception);
        Assert.Equal(EmployeeId.ToString(), entry.GetStructuredStateValue("EmployeeId"));
        Assert.Equal(nameof(AuditAction.Created), entry.GetStructuredStateValue("Action"));
    }
}
