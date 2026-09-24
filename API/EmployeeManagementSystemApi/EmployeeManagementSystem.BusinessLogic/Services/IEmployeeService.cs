using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IEmployeeService
{
    Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployeesAsync(GetEmployeesRequest request, CancellationToken cancellationToken = default);

    Task<ResponseModel<string>> GetEmployeesForExportAsync(ExportEmployeesRequest request, CancellationToken cancellationToken = default);

    Task<ResponseModel<EmployeeModel>> GetEmployeeAsync(string? searchTerm, CancellationToken cancellationToken = default);

    Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLogAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLogAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<ResponseModel<Guid?>> CreateEmployeeAsync(CreateEmployeeRequest request, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> UpdateEmployeeAsync(UpdateEmployeeRequest request, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> ReactivateEmployeeAsync(Guid employeeId, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeleteEmployeeAsync(Guid employeeId, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeleteAllEmployeeAuditLogAsync(CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> CreateEmployeeSalaryAsync(CreateSalaryRequest request, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
