using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Departments;

public class GetDepartmentHandler(IDepartmentRepository departments)
{
    public async Task<ResponseModel<DepartmentModel>> HandleAsync(Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        if (departmentId == Guid.Empty)
            return new ResponseModel<DepartmentModel>(400, "A valid department ID is required.");

        return await departments.GetDepartmentAsync(departmentId, cancellationToken);
    }
}
