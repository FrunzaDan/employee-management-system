using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.AuditLog;

public class GetEmployeeAuditLogHandler(IAuditLogRepository auditLog)
{
    public async Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> HandleAsync(Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<AuditLogEntry>>(400, "A valid employee ID is required.");

        return await auditLog.GetEmployeeAuditLogAsync(employeeId, cancellationToken);
    }
}
