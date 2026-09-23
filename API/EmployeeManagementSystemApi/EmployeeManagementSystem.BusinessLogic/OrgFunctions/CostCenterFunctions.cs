using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.OrgFunctions;

public class CostCenterFunctions(IDbUtils dbUtils)
{
    public async Task<ResponseModel<object>> CreateCostCenterFunction(CostCenterModel request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CostCenterCode))
            return new ResponseModel<object>(400, "Cost center code is required.");
        if (request.CostCenterCode.Length > FieldLengthConstants.CostCenterCode)
            return new ResponseModel<object>(400, "Cost center code is too long.");
        if (!string.IsNullOrEmpty(request.CostCenterName) &&
            request.CostCenterName.Length > FieldLengthConstants.CostCenterName)
            return new ResponseModel<object>(400, "Cost center name is too long.");

        request.Guid = SequentialGuid.NewGuid();

        return await dbUtils.CreateCostCenter(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetCostCenterFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");

        return await dbUtils.GetCostCenter(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetCostCentersFunction(CancellationToken cancellationToken = default) =>
        await dbUtils.GetCostCenters(cancellationToken);

    public async Task<ResponseModel<object>> EditCostCenterFunction(CostCenterModel request,
        CancellationToken cancellationToken = default)
    {
        if (request.Guid is null || request.Guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");
        if (!string.IsNullOrEmpty(request.CostCenterCode) &&
            request.CostCenterCode.Length > FieldLengthConstants.CostCenterCode)
            return new ResponseModel<object>(400, "Cost center code is too long.");
        if (!string.IsNullOrEmpty(request.CostCenterName) &&
            request.CostCenterName.Length > FieldLengthConstants.CostCenterName)
            return new ResponseModel<object>(400, "Cost center name is too long.");

        return await dbUtils.EditCostCenter(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> DeleteCostCenterFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");

        return await dbUtils.DeleteCostCenter(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetEmployeesByCostCenterFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");

        return await dbUtils.GetEmployeesByCostCenter(guid, cancellationToken);
    }
}
