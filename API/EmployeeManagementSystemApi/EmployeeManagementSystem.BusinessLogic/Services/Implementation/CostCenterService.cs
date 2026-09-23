using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class CostCenterService(CostCenterFunctions costCenterFunctions) : ICostCenterService
{
    public async Task<ResponseModel<Guid?>> CreateCostCenter(CreateCostCenterRequest request,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.CreateCostCenterFunction(request, cancellationToken);

    public async Task<ResponseModel<CostCenterModel>> GetCostCenter(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetCostCenterFunction(costCenterId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCenters(
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetCostCentersFunction(cancellationToken);

    public async Task<ResponseModel<object>> UpdateCostCenter(UpdateCostCenterRequest request,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.UpdateCostCenterFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteCostCenter(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.DeleteCostCenterFunction(costCenterId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenter(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetEmployeesByCostCenterFunction(costCenterId, cancellationToken);
}
