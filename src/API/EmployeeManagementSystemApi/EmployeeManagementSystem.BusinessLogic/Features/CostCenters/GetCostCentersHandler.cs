using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.CostCenters;

public class GetCostCentersHandler(ICostCenterRepository costCenters)
{
    public async Task<ResponseModel<IReadOnlyList<CostCenterModel>>> HandleAsync(
        CancellationToken cancellationToken = default) =>
        await costCenters.GetCostCentersAsync(cancellationToken);
}
