using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IDepartmentService
{
    Task<ResponseModel<object>> CreateDepartment(DepartmentModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetDepartment(string guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetDepartments(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> EditDepartment(DepartmentModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteDepartment(string guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetEmployeesByDepartment(string guid, CancellationToken cancellationToken = default);
}
