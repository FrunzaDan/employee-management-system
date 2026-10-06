using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Employees;

public class GetEmployeeInsightsHandler(IEmployeeRepository employees)
{
    public async Task<ResponseModel<EmployeeInsightsModel>> HandleAsync(
        CancellationToken cancellationToken = default) =>
        await employees.GetEmployeeInsightsAsync(cancellationToken);
}
