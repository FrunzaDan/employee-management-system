using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Constants;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.AuditLog;

public class GetAllEmployeeAuditLogHandler(IAuditLogRepository auditLog)
{
    public async Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> HandleAsync(int pageNumber,
        int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            return new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(400, "Page number must be 1 or greater.");

        if (pageSize < 1 || pageSize > PagingConstants.MaxPageSize)
            return new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(400,
                $"Page size must be between 1 and {PagingConstants.MaxPageSize}.");

        return await auditLog.GetAllEmployeeAuditLogAsync(pageNumber, pageSize, cancellationToken);
    }
}
