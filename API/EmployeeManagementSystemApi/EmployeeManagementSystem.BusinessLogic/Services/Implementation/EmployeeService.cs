using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class EmployeeService(
    EmployeeCreation employeeCreation,
    EmployeeGetting employeeGetting,
    EmployeeUpdating employeeUpdating,
    EmployeeActivation employeeActivation,
    EmployeeDeletion employeeDeletion,
    EmployeeSalary employeeSalary)
    : IEmployeeService
{
    public async Task<ResponseModel<object>> CreateEmployeeSalaryAsync(CreateSalaryRequest request, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeSalary.CreateEmployeeSalaryAsync(request, performedBy, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistoryAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await employeeSalary.GetEmployeeSalaryHistoryAsync(employeeId, cancellationToken);

    public async Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeActivation.DeactivateEmployeeAsync(employeeId, performedBy, cancellationToken);

    public async Task<ResponseModel<object>> ReactivateEmployeeAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeActivation.ReactivateEmployeeAsync(employeeId, performedBy, cancellationToken);

    public async Task<ResponseModel<object>> DeleteEmployeeAsync(Guid employeeId, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeDeletion.DeleteEmployeeAsync(employeeId, performedBy, cancellationToken);

    public async Task<ResponseModel<object>> UpdateEmployeeAsync(UpdateEmployeeRequest request, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeUpdating.UpdateEmployeeAsync(request, performedBy, cancellationToken);

    public async Task<ResponseModel<EmployeeModel>> GetEmployeeAsync(string? searchTerm,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeeAsync(searchTerm, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLogAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeeAuditLogAsync(employeeId, cancellationToken);

    public async Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLogAsync(int pageNumber,
        int pageSize, CancellationToken cancellationToken = default) =>
        await employeeGetting.GetAllEmployeeAuditLogAsync(pageNumber, pageSize, cancellationToken);

    public async Task<ResponseModel<object>> DeleteAllEmployeeAuditLogAsync(
        CancellationToken cancellationToken = default) =>
        await employeeDeletion.DeleteAllEmployeeAuditLogAsync(cancellationToken);

    public async Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployeesAsync(GetEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeesAsync(request, cancellationToken);

    public async Task<ResponseModel<string>> GetEmployeesForExportAsync(ExportEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        await employeeGetting.GetEmployeesForExportAsync(request, cancellationToken);

    public async Task<ResponseModel<Guid?>> CreateEmployeeAsync(CreateEmployeeRequest request, string performedBy,
        CancellationToken cancellationToken = default) =>
        await employeeCreation.CreateEmployeeAsync(request, performedBy, cancellationToken);
}
