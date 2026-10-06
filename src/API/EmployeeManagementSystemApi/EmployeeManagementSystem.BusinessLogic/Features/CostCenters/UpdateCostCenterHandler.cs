using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Constants;

namespace EmployeeManagementSystem.BusinessLogic.Features.CostCenters;

public class UpdateCostCenterHandler(ICostCenterRepository costCenters)
{
    public async Task<ResponseModel<object>> HandleAsync(UpdateCostCenterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CostCenterId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid cost center ID is required.");
        if (request.Code is not null && string.IsNullOrWhiteSpace(request.Code))
            return new ResponseModel<object>(400, "Cost center code cannot be blank.");
        if (request.Code?.Length > FieldLengthConstants.CostCenterCode)
            return new ResponseModel<object>(400, "Cost center code is too long.");
        if (request.Name?.Length > FieldLengthConstants.CostCenterName)
            return new ResponseModel<object>(400, "Cost center name is too long.");

        return await costCenters.UpdateCostCenterAsync(request, cancellationToken);
    }
}
