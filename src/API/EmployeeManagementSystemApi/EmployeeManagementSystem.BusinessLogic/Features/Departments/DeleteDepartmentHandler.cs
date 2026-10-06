using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;

namespace EmployeeManagementSystem.BusinessLogic.Features.Departments;

public class DeleteDepartmentHandler(IDepartmentRepository departments)
{
    public async Task<ResponseModel<object>> HandleAsync(Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        if (departmentId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid department ID is required.");

        return await departments.DeleteDepartmentAsync(departmentId, cancellationToken);
    }
}
