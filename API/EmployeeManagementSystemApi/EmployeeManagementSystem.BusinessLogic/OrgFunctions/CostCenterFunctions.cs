using EmployeeManagementSystem.BusinessLogic.Constants;
using EmployeeManagementSystem.BusinessLogic.Validations;
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

        request.Guid = Guid.NewGuid().ToString();

        return await dbUtils.CreateCostCenter(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetCostCenterFunction(string guid,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(guid) || !GuidValidation.ValidateGuid(guid))
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");

        return await dbUtils.GetCostCenter(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetCostCentersFunction(CancellationToken cancellationToken = default) =>
        await dbUtils.GetCostCenters(cancellationToken);

    public async Task<ResponseModel<object>> EditCostCenterFunction(CostCenterModel request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.Guid) || !GuidValidation.ValidateGuid(request.Guid))
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");
        if (!string.IsNullOrEmpty(request.CostCenterCode) &&
            request.CostCenterCode.Length > FieldLengthConstants.CostCenterCode)
            return new ResponseModel<object>(400, "Cost center code is too long.");
        if (!string.IsNullOrEmpty(request.CostCenterName) &&
            request.CostCenterName.Length > FieldLengthConstants.CostCenterName)
            return new ResponseModel<object>(400, "Cost center name is too long.");

        return await dbUtils.EditCostCenter(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> DeleteCostCenterFunction(string guid,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(guid) || !GuidValidation.ValidateGuid(guid))
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");

        return await dbUtils.DeleteCostCenter(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetEmployeesByCostCenterFunction(string guid,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(guid) || !GuidValidation.ValidateGuid(guid))
            return new ResponseModel<object>(400, "A valid cost center GUID is required.");

        return await dbUtils.GetEmployeesByCostCenter(guid, cancellationToken);
    }
}
