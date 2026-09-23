using EmployeeManagementSystem.DataAccess.DBConnection;
using Microsoft.Extensions.Logging;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

// Writing an audit entry is best-effort: it always runs after the employee
// mutation it's recording has already succeeded, so a DB hiccup while writing
// the log must never turn an otherwise-successful request into a 500 — it's
// swallowed and logged instead.
public class EmployeeAuditLogger(IDbUtils dbUtils, ILogger<EmployeeAuditLogger> logger) : IEmployeeAuditLogger
{
    public async Task Log(Guid employeeGuid, string employerId, string action, string? details = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbUtils.LogEmployeeAudit(employeeGuid, employerId, action, details, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to write audit log entry for employee {EmployeeGuid}, action {Action}",
                employeeGuid, action);
        }
    }
}
