using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeActivation(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> DeactivateEmployee(Guid guid, string employerId,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty Guid.");

        var response = await dbUtils.DeactivateEmployee(guid, cancellationToken);

        // Not forwarding cancellationToken to the audit write: the mutation already
        // succeeded, so the log entry should still be attempted regardless of whether
        // the client that triggered it is still connected.
        if (response.Status == 200)
            await auditLogger.Log(guid, employerId, "Deactivated");

        return response;
    }

    public async Task<ResponseModel<object>> ReactivateEmployee(Guid guid, string employerId,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty Guid.");

        var response = await dbUtils.ReactivateEmployee(guid, cancellationToken);

        if (response.Status == 200)
            await auditLogger.Log(guid, employerId, "Reactivated");

        return response;
    }
}