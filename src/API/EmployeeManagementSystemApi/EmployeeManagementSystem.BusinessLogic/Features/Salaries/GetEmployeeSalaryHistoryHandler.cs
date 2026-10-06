using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Salaries;

public class GetEmployeeSalaryHistoryHandler(ISalaryRepository salaries)
{
    public async Task<ResponseModel<IReadOnlyList<SalaryModel>>> HandleAsync(Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<SalaryModel>>(400, "A valid employee ID is required.");

        return await salaries.GetEmployeeSalaryHistoryAsync(employeeId, cancellationToken);
    }
}
