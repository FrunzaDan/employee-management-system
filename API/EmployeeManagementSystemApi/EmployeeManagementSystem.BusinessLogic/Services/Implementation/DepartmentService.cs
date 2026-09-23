using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class DepartmentService(DepartmentFunctions departmentFunctions) : IDepartmentService
{
    public async Task<ResponseModel<Guid?>> CreateDepartment(CreateDepartmentRequest request,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.CreateDepartmentFunction(request, cancellationToken);

    public async Task<ResponseModel<DepartmentModel>> GetDepartment(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetDepartmentFunction(departmentId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartments(
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetDepartmentsFunction(cancellationToken);

    public async Task<ResponseModel<object>> UpdateDepartment(UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.UpdateDepartmentFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteDepartment(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.DeleteDepartmentFunction(departmentId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartment(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetEmployeesByDepartmentFunction(departmentId, cancellationToken);
}
