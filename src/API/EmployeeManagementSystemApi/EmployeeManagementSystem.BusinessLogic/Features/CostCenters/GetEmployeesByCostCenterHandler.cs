using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.CostCenters;

public class GetEmployeesByCostCenterHandler(ICostCenterRepository costCenters)
{
    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> HandleAsync(Guid costCenterId,
        CancellationToken cancellationToken = default)
    {
        if (costCenterId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(400, "A valid cost center ID is required.");

        return await costCenters.GetEmployeesByCostCenterAsync(costCenterId, cancellationToken);
    }
}
