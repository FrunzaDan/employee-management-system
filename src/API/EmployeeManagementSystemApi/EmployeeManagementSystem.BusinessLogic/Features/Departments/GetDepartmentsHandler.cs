using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Departments;

public class GetDepartmentsHandler(IDepartmentRepository departments)
{
    public async Task<ResponseModel<IReadOnlyList<DepartmentModel>>> HandleAsync(
        CancellationToken cancellationToken = default) =>
        await departments.GetDepartmentsAsync(cancellationToken);
}
