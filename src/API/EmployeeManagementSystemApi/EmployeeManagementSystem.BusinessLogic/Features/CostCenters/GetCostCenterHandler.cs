using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.CostCenters;

public class GetCostCenterHandler(ICostCenterRepository costCenters)
{
    public async Task<ResponseModel<CostCenterModel>> HandleAsync(Guid costCenterId,
        CancellationToken cancellationToken = default)
    {
        if (costCenterId == Guid.Empty)
            return new ResponseModel<CostCenterModel>(400, "A valid cost center ID is required.");

        return await costCenters.GetCostCenterAsync(costCenterId, cancellationToken);
    }
}
