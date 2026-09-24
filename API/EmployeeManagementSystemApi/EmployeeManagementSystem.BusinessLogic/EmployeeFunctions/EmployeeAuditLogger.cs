using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public partial class EmployeeAuditLogger(IDbUtils dbUtils, ILogger<EmployeeAuditLogger> logger) : IEmployeeAuditLogger
{
    public async Task LogAsync(Guid employeeId, string performedBy, AuditAction action, string? details = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbUtils.LogEmployeeAuditAsync(employeeId, performedBy, action, details, cancellationToken);
        }
        catch (Exception ex)
        {
            LogAuditWriteFailed(logger, ex, employeeId, action);
        }
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Error,
        Message = "Failed to write audit log entry for employee {EmployeeId}, action {Action}")]
    private static partial void LogAuditWriteFailed(ILogger logger, Exception exception, Guid employeeId,
        AuditAction action);
}
