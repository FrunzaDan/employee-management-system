using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Abstractions;

/// <summary>Department persistence. Implemented by DataAccess (stored procedures).</summary>
public interface IDepartmentRepository
{
    Task<ResponseModel<Guid?>> CreateDepartmentAsync(CreateDepartmentRequest department, CancellationToken cancellationToken = default);
    Task<ResponseModel<DepartmentModel>> GetDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateDepartmentAsync(UpdateDepartmentRequest department, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
