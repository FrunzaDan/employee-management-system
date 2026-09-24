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

        if (response.Status == 200)
            await auditLogger.LogAsync(employeeId, performedBy, AuditAction.Deleted);

        return response;
    }

    public async Task<ResponseModel<object>> DeleteAllEmployeeAuditLogAsync(CancellationToken cancellationToken = default) =>
        await dbUtils.DeleteAllEmployeeAuditLogAsync(cancellationToken);
}