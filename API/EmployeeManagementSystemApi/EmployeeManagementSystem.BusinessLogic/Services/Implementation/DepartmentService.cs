using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class DepartmentService(DepartmentFunctions departmentFunctions) : IDepartmentService
{
    public async Task<ResponseModel<Guid?>> CreateDepartmentAsync(CreateDepartmentRequest request,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.CreateDepartmentAsync(request, cancellationToken);

    public async Task<ResponseModel<DepartmentModel>> GetDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetDepartmentAsync(departmentId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartmentsAsync(
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetDepartmentsAsync(cancellationToken);

    public async Task<ResponseModel<object>> UpdateDepartmentAsync(UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.UpdateDepartmentAsync(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.DeleteDepartmentAsync(departmentId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        await departmentFunctions.GetEmployeesByDepartmentAsync(departmentId, cancellationToken);
}
