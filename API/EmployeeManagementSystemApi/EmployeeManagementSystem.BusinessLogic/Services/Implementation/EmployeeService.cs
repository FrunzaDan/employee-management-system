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
    public async Task<ResponseModel<object>> AddEmployeeSalary(CreateSalaryRequest request, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeSalary.AddSalaryFunction(request, performedBy, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistory(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await employeeSalary.GetSalaryHistoryFunction(employeeId, cancellationToken);

    public async Task<ResponseModel<object>> DeactivateEmployee(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeActivation.DeactivateEmployee(employeeId, performedBy, cancellationToken);

    public async Task<ResponseModel<object>> ReactivateEmployee(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeActivation.ReactivateEmployee(employeeId, performedBy, cancellationToken);

    public async Task<ResponseModel<object>> DeleteEmployee(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeDeletion.DeleteEmployee(employeeId, performedBy, cancellationToken);

    public async Task<ResponseModel<object>> EditEmployee(UpdateEmployeeRequest request, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeEditing.EditEmployeeFunction(request, performedBy, cancellationToken);

    public async Task<ResponseModel<EmployeeModel>> GetEmployee(string? searchTerm,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeeFunction(searchTerm, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLog(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeeAuditLogFunction(employeeId, cancellationToken);

    public async Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLog(int pageNumber,
        int pageSize, CancellationToken cancellationToken = default) =>
        await employeeGetting.GetAllAuditLogFunction(pageNumber, pageSize, cancellationToken);

    public async Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(
        CancellationToken cancellationToken = default) =>
        await employeeDeletion.DeleteAllAuditLogFunction(cancellationToken);

    public async Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployees(GetEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeesFunction(request, cancellationToken);

    public async Task<ResponseModel<string>> GetEmployeesForExport(ExportEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeesForExportFunction(request, cancellationToken);

    public async Task<ResponseModel<Guid?>> RegisterEmployee(CreateEmployeeRequest request, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeRegistration.RegisterEmployeeFunction(request, performedBy, cancellationToken);
}
