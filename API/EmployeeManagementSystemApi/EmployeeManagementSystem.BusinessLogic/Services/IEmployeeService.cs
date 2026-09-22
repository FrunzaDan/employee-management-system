using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IEmployeeService
{
    Task<ResponseModel<object>> GetEmployees(GetEmployeesRequest request, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> GetEmployeesForExport(ExportEmployeesRequest request, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> GetEmployee(GetEmployeeRequest request, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> GetEmployeeAuditLog(string employeeGuid, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> GetAllEmployeeAuditLog(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> RegisterEmployee(EmployeeModel request, string employerId, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> EditEmployee(EmployeeModel request, string employerId, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeactivateEmployee(string guid, string employerId, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> ReactivateEmployee(string guid, string employerId, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeleteEmployee(string guid, string employerId, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> AddEmployeeSalary(SalaryHistoryEntry request, string employerId, CancellationToken cancellationToken = default);

    Task<ResponseModel<object>> GetEmployeeSalaryHistory(string employeeGuid, CancellationToken cancellationToken = default);
}