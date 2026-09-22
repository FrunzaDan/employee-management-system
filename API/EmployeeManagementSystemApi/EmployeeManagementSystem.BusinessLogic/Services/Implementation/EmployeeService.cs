using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class EmployeeService(
    EmployeeRegistration employeeRegistration,
    EmployeeGetting employeeGetting,
    EmployeeEditing employeeEditing,
    EmployeeActivation employeeActivation,
    EmployeeDeletion employeeDeletion,
    EmployeeSalary employeeSalary)
    : IEmployeeService
{
    public async Task<ResponseModel<object>> AddEmployeeSalary(SalaryHistoryEntry request, string employerId,
        CancellationToken cancellationToken = default) =>
        await employeeSalary.AddSalaryFunction(request, employerId, cancellationToken);

    public async Task<ResponseModel<object>> GetEmployeeSalaryHistory(string employeeGuid,
        CancellationToken cancellationToken = default) =>
        await employeeSalary.GetSalaryHistoryFunction(employeeGuid, cancellationToken);

    public async Task<ResponseModel<object>> DeactivateEmployee(string employeeGuid, string employerId,
        CancellationToken cancellationToken = default) =>
        await employeeActivation.DeactivateEmployee(employeeGuid, employerId, cancellationToken);

    public async Task<ResponseModel<object>> ReactivateEmployee(string employeeGuid, string employerId,
        CancellationToken cancellationToken = default) =>
        await employeeActivation.ReactivateEmployee(employeeGuid, employerId, cancellationToken);

    public async Task<ResponseModel<object>> DeleteEmployee(string employeeGuid, string employerId,
        CancellationToken cancellationToken = default) =>
        await employeeDeletion.DeleteEmployee(employeeGuid, employerId, cancellationToken);

    public async Task<ResponseModel<object>> EditEmployee(EmployeeModel editEmployeeRequest, string employerId,
        CancellationToken cancellationToken = default) =>
        await employeeEditing.EditEmployeeFunction(editEmployeeRequest, employerId, cancellationToken);

    public async Task<ResponseModel<object>> GetEmployee(GetEmployeeRequest getEmployeeRqst,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeeFunction(getEmployeeRqst, cancellationToken);

    public async Task<ResponseModel<object>> GetEmployeeAuditLog(string employeeGuid,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeeAuditLogFunction(employeeGuid, cancellationToken);

    public async Task<ResponseModel<object>> GetAllEmployeeAuditLog(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetAllAuditLogFunction(pageNumber, pageSize, cancellationToken);

    public async Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(
        CancellationToken cancellationToken = default) =>
        await employeeDeletion.DeleteAllAuditLogFunction(cancellationToken);

    public async Task<ResponseModel<object>> GetEmployees(GetEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeesFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> GetEmployeesForExport(ExportEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeesForExportFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> RegisterEmployee(EmployeeModel employeeRqst, string employerId,
        CancellationToken cancellationToken = default) =>
        await employeeRegistration.RegisterEmployeeFunction(employeeRqst, employerId, cancellationToken);
}
