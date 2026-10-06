using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Constants;

namespace EmployeeManagementSystem.BusinessLogic.Features.CostCenters;

public class CreateCostCenterHandler(ICostCenterRepository costCenters)
{
    public async Task<ResponseModel<Guid?>> HandleAsync(CreateCostCenterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return new ResponseModel<Guid?>(400, "Cost center code is required.");
        if (request.Code.Length > FieldLengthConstants.CostCenterCode)
            return new ResponseModel<Guid?>(400, "Cost center code is too long.");
        if (request.Name?.Length > FieldLengthConstants.CostCenterName)
            return new ResponseModel<Guid?>(400, "Cost center name is too long.");

        return await costCenters.CreateCostCenterAsync(request, cancellationToken);
    }
}
