using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;

namespace EmployeeManagementSystem.BusinessLogic.Features.CostCenters;

public class DeleteCostCenterHandler(ICostCenterRepository costCenters)
{
    public async Task<ResponseModel<object>> HandleAsync(Guid costCenterId,
        CancellationToken cancellationToken = default)
    {
        if (costCenterId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid cost center ID is required.");

        return await costCenters.DeleteCostCenterAsync(costCenterId, cancellationToken);
    }
}
