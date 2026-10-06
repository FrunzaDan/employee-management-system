using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Departments;

public class GetEmployeesByDepartmentHandler(IDepartmentRepository departments)
{
    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> HandleAsync(Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        if (departmentId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(400, "A valid department ID is required.");

        return await departments.GetEmployeesByDepartmentAsync(departmentId, cancellationToken);
    }
}
