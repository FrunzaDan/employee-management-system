using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public interface IEmployeeAuditLogger
{
    Task Log(Guid employeeId, string performedBy, AuditAction action, string? details = null,
        CancellationToken cancellationToken = default);
}
