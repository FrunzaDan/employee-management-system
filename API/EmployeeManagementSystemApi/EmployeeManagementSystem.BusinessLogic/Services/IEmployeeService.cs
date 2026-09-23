using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IEmployeeService
{
    Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployees(GetEmployeesRequest request, CancellationToken cancellationToken = default);

    Task<ResponseModel<string>> GetEmployeesForExport(ExportEmployeesRequest request, CancellationToken cancellationToken = default);

    Task<ResponseModel<EmployeeModel>> GetEmployee(string? searchTerm, CancellationToken cancellationToken = default);

    Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLog(Guid employeeId, CancellationToken cancellationToken = default);

    Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLog(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<ResponseModel<Guid?>> CreateEmployee(CreateEmployeeRequest request, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> UpdateEmployee(UpdateEmployeeRequest request, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeactivateEmployee(Guid employeeId, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> ReactivateEmployee(Guid employeeId, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeleteEmployee(Guid employeeId, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> CreateEmployeeSalary(CreateSalaryRequest request, string performedBy, CancellationToken cancellationToken = default);

    Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistory(Guid employeeId, CancellationToken cancellationToken = default);
}
