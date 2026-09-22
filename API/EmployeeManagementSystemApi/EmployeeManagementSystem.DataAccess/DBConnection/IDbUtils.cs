using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public interface IDbUtils
{
    public Task<ResponseModel<object>> RegisterEmployee(EmployeeModel employee, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployee(GetEmployeeRequest getEmployeeRqst, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployees(GetEmployeesRequest request, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditEmployee(EmployeeModel editEmployeeRqst, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeactivateEmployee(string employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> ReactivateEmployee(string employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteEmployee(string employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<int?>> CheckEmployerCredentialsFromDb(EmployerCredentials employerCredentials, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> LogEmployeeAudit(string employeeGuid, string employerId, string action, string? details, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeeAuditLog(string employeeGuid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetAllEmployeeAuditLog(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> AddEmployeeSalary(SalaryHistoryEntry entry, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeeSalaryHistory(string employeeGuid, CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> CreateOffice(OfficeModel office, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetOffice(string guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetOffices(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditOffice(OfficeModel office, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteOffice(string guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeesByOffice(string officeGuid, CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> CreateDepartment(DepartmentModel department, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetDepartment(string guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetDepartments(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditDepartment(DepartmentModel department, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteDepartment(string guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeesByDepartment(string departmentGuid, CancellationToken cancellationToken = default);

    public Task<ResponseModel<object>> CreateCostCenter(CostCenterModel costCenter, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetCostCenter(string guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetCostCenters(CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> EditCostCenter(CostCenterModel costCenter, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> DeleteCostCenter(string guid, CancellationToken cancellationToken = default);
    public Task<ResponseModel<object>> GetEmployeesByCostCenter(string costCenterGuid, CancellationToken cancellationToken = default);
}