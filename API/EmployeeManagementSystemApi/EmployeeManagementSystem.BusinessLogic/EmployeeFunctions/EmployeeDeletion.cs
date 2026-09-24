using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeDeletion(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> DeleteEmployeeAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty employee ID.");

        var response = await dbUtils.DeleteEmployeeAsync(employeeId, cancellationToken);

        // No FK from EmployeeAuditLog to Employee, deliberately — this row
        // is the one place that outlives the employee it's about. Also not forwarding
        // cancellationToken here: the delete already succeeded, so the log entry should
        // still be attempted even if the client has since disconnected.
        if (response.Status == 200)
            await auditLogger.LogAsync(employeeId, performedBy, AuditAction.Deleted);

        return response;
    }

    // Mirrors GetAllEmployeeAuditLogAsync living in EmployeeGetting: grouped by verb, not
    // by entity, alongside the per-employee delete above. Not audit-logged itself —
    // there's no EmployeeId to attach the entry to once the table is wiped.
    public async Task<ResponseModel<object>> DeleteAllEmployeeAuditLogAsync(CancellationToken cancellationToken = default) =>
        await dbUtils.DeleteAllEmployeeAuditLogAsync(cancellationToken);
}