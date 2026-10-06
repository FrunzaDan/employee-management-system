using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;

namespace EmployeeManagementSystem.BusinessLogic.Features.AuditLog;

public class DeleteAllEmployeeAuditLogHandler(IAuditLogRepository auditLog)
{
    public async Task<ResponseModel<object>> HandleAsync(CancellationToken cancellationToken = default) =>
        await auditLog.DeleteAllEmployeeAuditLogAsync(cancellationToken);
}
