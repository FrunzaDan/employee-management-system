using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

// Writing an audit entry is best-effort: it always runs after the employee
// mutation it's recording has already succeeded, so a DB hiccup while writing
// the log must never turn an otherwise-successful request into a 500 — it's
// swallowed and logged instead.
public partial class EmployeeAuditLogger(IDbUtils dbUtils, ILogger<EmployeeAuditLogger> logger) : IEmployeeAuditLogger
{
    public async Task Log(Guid employeeId, string performedBy, AuditAction action, string? details = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbUtils.LogEmployeeAudit(employeeId, performedBy, action, details, cancellationToken);
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
