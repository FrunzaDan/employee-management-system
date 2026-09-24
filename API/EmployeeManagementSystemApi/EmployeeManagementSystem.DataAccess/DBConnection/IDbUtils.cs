using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public interface IDbUtils
{
    Task<ResponseModel<Guid?>> CreateEmployeeAsync(CreateEmployeeRequest employee, CancellationToken cancellationToken = default);
    Task<ResponseModel<EmployeeModel>> GetEmployeeAsync(EmployeeLookup lookup, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployeesAsync(GetEmployeesRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateEmployeeAsync(UpdateEmployeeRequest employee, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> ReactivateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<EmployerRole?>> CheckEmployerCredentialsFromDbAsync(EmployerCredentials employerCredentials, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> LogEmployeeAuditAsync(Guid employeeId, string performedBy, AuditAction action, string? details, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLogAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLogAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteAllEmployeeAuditLogAsync(CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> CreateEmployeeSalaryAsync(CreateSalaryRequest salary, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<ResponseModel<Guid?>> CreateOfficeAsync(CreateOfficeRequest office, CancellationToken cancellationToken = default);
    Task<ResponseModel<OfficeModel>> GetOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOfficesAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateOfficeAsync(UpdateOfficeRequest office, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);

    Task<ResponseModel<Guid?>> CreateDepartmentAsync(CreateDepartmentRequest department, CancellationToken cancellationToken = default);
    Task<ResponseModel<DepartmentModel>> GetDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateDepartmentAsync(UpdateDepartmentRequest department, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);

    Task<ResponseModel<Guid?>> CreateCostCenterAsync(CreateCostCenterRequest costCenter, CancellationToken cancellationToken = default);
    Task<ResponseModel<CostCenterModel>> GetCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCentersAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateCostCenterAsync(UpdateCostCenterRequest costCenter, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
}
