using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public interface IEmployeeAuditLogger
{
    // Callers pass CancellationToken.None: the change is already saved, so its audit entry is written even if the request is cancelled.
    Task LogAsync(Guid employeeId, string performedBy, AuditAction action, string? details = null,
        CancellationToken cancellationToken = default);
}
