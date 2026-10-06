using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Abstractions;

/// <summary>How a single employee is looked up: exactly one of the three is set.</summary>
public sealed record EmployeeLookup(Guid? EmployeeId = null, string? PhoneNumber = null, string? Email = null);

/// <summary>Employee persistence. Implemented by DataAccess (stored procedures).</summary>
public interface IEmployeeRepository
{
    Task<ResponseModel<Guid?>> CreateEmployeeAsync(CreateEmployeeRequest employee, CancellationToken cancellationToken = default);
    Task<ResponseModel<EmployeeModel>> GetEmployeeAsync(EmployeeLookup lookup, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployeesAsync(GetEmployeesRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateEmployeeAsync(UpdateEmployeeRequest employee, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> ReactivateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<EmployeeInsightsModel>> GetEmployeeInsightsAsync(CancellationToken cancellationToken = default);
}
