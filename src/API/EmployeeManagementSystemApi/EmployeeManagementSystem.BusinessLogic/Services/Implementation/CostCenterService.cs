using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class CostCenterService(CostCenterFunctions costCenterFunctions) : ICostCenterService
{
    public async Task<ResponseModel<Guid?>> CreateCostCenterAsync(CreateCostCenterRequest request,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.CreateCostCenterAsync(request, cancellationToken);

    public async Task<ResponseModel<CostCenterModel>> GetCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetCostCenterAsync(costCenterId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCentersAsync(
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetCostCentersAsync(cancellationToken);

    public async Task<ResponseModel<object>> UpdateCostCenterAsync(UpdateCostCenterRequest request,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.UpdateCostCenterAsync(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.DeleteCostCenterAsync(costCenterId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetEmployeesByCostCenterAsync(costCenterId, cancellationToken);
}
