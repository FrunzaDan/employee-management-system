using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeActivation(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default)
    {
        // A malformed GUID never gets this far (model binding rejects it); Guid.Empty is what
        // a missing one binds to.
        if (employeeId == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty employee ID.");

        var response = await dbUtils.DeactivateEmployeeAsync(employeeId, cancellationToken);

        // Not forwarding cancellationToken to the audit write: the mutation already
        // succeeded, so the log entry should still be attempted regardless of whether
        // the client that triggered it is still connected.
        if (response.Status == 200)
            await auditLogger.LogAsync(employeeId, performedBy, AuditAction.Deactivated);

        return response;
    }

    public async Task<ResponseModel<object>> ReactivateEmployeeAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty employee ID.");

        var response = await dbUtils.ReactivateEmployeeAsync(employeeId, cancellationToken);

        if (response.Status == 200)
            await auditLogger.LogAsync(employeeId, performedBy, AuditAction.Reactivated);

        return response;
    }
}
