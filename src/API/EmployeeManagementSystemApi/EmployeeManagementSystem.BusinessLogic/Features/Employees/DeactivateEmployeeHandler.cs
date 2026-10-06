using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Employees;

public class DeactivateEmployeeHandler(IEmployeeRepository employees, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> HandleAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty employee ID.");

        var response = await employees.DeactivateEmployeeAsync(employeeId, cancellationToken);

        if (response.Status == 200)
            await auditLogger.LogAsync(employeeId, performedBy, AuditAction.Deactivated, cancellationToken: CancellationToken.None);

        return response;
    }
}
