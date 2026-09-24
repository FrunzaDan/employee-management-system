using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IDepartmentService
{
    Task<ResponseModel<Guid?>> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<DepartmentModel>> GetDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateDepartmentAsync(UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
