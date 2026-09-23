using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public interface IDbUtils
{
    public Task<ResponseModel<object>> RegisterEmployee(EmployeeModel employee, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployee(GetEmployeeRequest getEmployeeRqst, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployees(GetEmployeesRequest request, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditEmployee(EmployeeModel editEmployeeRqst, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeactivateEmployee(Guid employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> ReactivateEmployee(Guid employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteEmployee(Guid employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<int?>> CheckEmployerCredentialsFromDb(EmployerCredentials employerCredentials, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> LogEmployeeAudit(Guid employeeGuid, string employerId, string action, string? details, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeeAuditLog(Guid employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetAllEmployeeAuditLog(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> AddEmployeeSalary(SalaryHistoryEntry entry, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeeSalaryHistory(Guid employeeGuid, CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> CreateOffice(OfficeModel office, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetOffice(Guid guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetOffices(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditOffice(OfficeModel office, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteOffice(Guid guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeesByOffice(Guid officeGuid, CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> CreateDepartment(DepartmentModel department, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetDepartment(Guid guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetDepartments(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditDepartment(DepartmentModel department, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteDepartment(Guid guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeesByDepartment(Guid departmentGuid, CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> CreateCostCenter(CostCenterModel costCenter, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetCostCenter(Guid guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetCostCenters(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditCostCenter(CostCenterModel costCenter, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteCostCenter(Guid guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeesByCostCenter(Guid costCenterGuid, CancellationToken cancellationToken = default);
}