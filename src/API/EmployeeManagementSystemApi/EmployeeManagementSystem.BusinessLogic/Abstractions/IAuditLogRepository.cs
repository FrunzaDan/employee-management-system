using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Abstractions;

/// <summary>Employee audit log persistence. Implemented by DataAccess (stored procedures).</summary>
public interface IAuditLogRepository
{
    Task<ResponseModel<object>> LogEmployeeAuditAsync(Guid employeeId, string performedBy, AuditAction action, string? details, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLogAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLogAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteAllEmployeeAuditLogAsync(CancellationToken cancellationToken = default);
}
