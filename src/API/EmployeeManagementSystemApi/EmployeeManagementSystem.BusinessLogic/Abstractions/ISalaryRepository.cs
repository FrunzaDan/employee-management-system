using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Abstractions;

/// <summary>Salary history persistence. Implemented by DataAccess (stored procedures).</summary>
public interface ISalaryRepository
{
    Task<ResponseModel<object>> CreateEmployeeSalaryAsync(CreateSalaryRequest salary, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
