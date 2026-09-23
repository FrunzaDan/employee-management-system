using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public interface IDbUtils
{
    public Task<ResponseModel<Guid?>> RegisterEmployee(CreateEmployeeRequest employee, CancellationToken cancellationToken = default);
    public Task<ResponseModel<EmployeeModel>> GetEmployee(EmployeeLookup lookup, CancellationToken cancellationToken = default);
    public Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployees(GetEmployeesRequest request, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditEmployee(UpdateEmployeeRequest employee, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeactivateEmployee(Guid employeeId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> ReactivateEmployee(Guid employeeId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteEmployee(Guid employeeId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<EmployerRole?>> CheckEmployerCredentialsFromDb(EmployerCredentials employerCredentials, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> LogEmployeeAudit(Guid employeeId, string performedBy, AuditAction action, string? details, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLog(Guid employeeId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLog(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> AddEmployeeSalary(CreateSalaryRequest salary, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistory(Guid employeeId, CancellationToken cancellationToken = default);

    public Task<ResponseModel<Guid?>> CreateOffice(CreateOfficeRequest office, CancellationToken cancellationToken = default);
    public Task<ResponseModel<OfficeModel>> GetOffice(Guid officeId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOffices(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditOffice(UpdateOfficeRequest office, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteOffice(Guid officeId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOffice(Guid officeId, CancellationToken cancellationToken = default);

    public Task<ResponseModel<Guid?>> CreateDepartment(CreateDepartmentRequest department, CancellationToken cancellationToken = default);
    public Task<ResponseModel<DepartmentModel>> GetDepartment(Guid departmentId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartments(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditDepartment(UpdateDepartmentRequest department, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteDepartment(Guid departmentId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartment(Guid departmentId, CancellationToken cancellationToken = default);

    public Task<ResponseModel<Guid?>> CreateCostCenter(CreateCostCenterRequest costCenter, CancellationToken cancellationToken = default);
    public Task<ResponseModel<CostCenterModel>> GetCostCenter(Guid costCenterId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCenters(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditCostCenter(UpdateCostCenterRequest costCenter, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteCostCenter(Guid costCenterId, CancellationToken cancellationToken = default);
    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenter(Guid costCenterId, CancellationToken cancellationToken = default);
}
