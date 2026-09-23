using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class DepartmentService(DepartmentFunctions departmentFunctions) : IDepartmentService
{
    public async Task<ResponseModel<object>> CreateDepartment(DepartmentModel request,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.CreateDepartmentFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> GetDepartment(Guid guid,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetDepartmentFunction(guid, cancellationToken);

    public async Task<ResponseModel<object>> GetDepartments(CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetDepartmentsFunction(cancellationToken);

    public async Task<ResponseModel<object>> EditDepartment(DepartmentModel request,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.EditDepartmentFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteDepartment(Guid guid,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.DeleteDepartmentFunction(guid, cancellationToken);

    public async Task<ResponseModel<object>> GetEmployeesByDepartment(Guid guid,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetEmployeesByDepartmentFunction(guid, cancellationToken);
}
