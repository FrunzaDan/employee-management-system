using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class CostCenterService(CostCenterFunctions costCenterFunctions) : ICostCenterService
{
    public async Task<ResponseModel<object>> CreateCostCenter(CostCenterModel request,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.CreateCostCenterFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> GetCostCenter(Guid guid,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetCostCenterFunction(guid, cancellationToken);

    public async Task<ResponseModel<object>> GetCostCenters(CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetCostCentersFunction(cancellationToken);

    public async Task<ResponseModel<object>> EditCostCenter(CostCenterModel request,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.EditCostCenterFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteCostCenter(Guid guid,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.DeleteCostCenterFunction(guid, cancellationToken);

    public async Task<ResponseModel<object>> GetEmployeesByCostCenter(Guid guid,
        CancellationToken cancellationToken = default) =>
        await costCenterFunctions.GetEmployeesByCostCenterFunction(guid, cancellationToken);
}
