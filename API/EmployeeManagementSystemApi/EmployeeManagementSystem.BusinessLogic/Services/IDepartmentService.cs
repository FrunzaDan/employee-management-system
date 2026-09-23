using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IDepartmentService
{
    Task<ResponseModel<object>> CreateDepartment(DepartmentModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetDepartment(Guid guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetDepartments(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> EditDepartment(DepartmentModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteDepartment(Guid guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetEmployeesByDepartment(Guid guid, CancellationToken cancellationToken = default);
}
