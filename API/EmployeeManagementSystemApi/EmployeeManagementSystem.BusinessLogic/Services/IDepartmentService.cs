using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IDepartmentService
{
    Task<ResponseModel<Guid?>> CreateDepartment(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<DepartmentModel>> GetDepartment(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartments(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateDepartment(UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteDepartment(Guid departmentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartment(Guid departmentId, CancellationToken cancellationToken = default);
}
