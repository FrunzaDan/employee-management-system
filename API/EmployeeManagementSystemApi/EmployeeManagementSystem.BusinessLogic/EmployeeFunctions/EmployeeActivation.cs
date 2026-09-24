using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeActivation(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty employee ID.");

        var response = await dbUtils.DeactivateEmployeeAsync(employeeId, cancellationToken);

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
