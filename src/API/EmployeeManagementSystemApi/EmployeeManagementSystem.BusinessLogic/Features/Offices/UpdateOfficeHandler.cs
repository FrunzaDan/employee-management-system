using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Constants;

namespace EmployeeManagementSystem.BusinessLogic.Features.Offices;

public class UpdateOfficeHandler(IOfficeRepository offices)
{
    public async Task<ResponseModel<object>> HandleAsync(UpdateOfficeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OfficeId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid office ID is required.");
        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            return new ResponseModel<object>(400, "Office name cannot be blank.");
        if (request.Name?.Length > FieldLengthConstants.OfficeName)
            return new ResponseModel<object>(400, "Office name is too long.");
        if (request.City?.Length > FieldLengthConstants.City)
            return new ResponseModel<object>(400, "City is too long.");
        if (request.Country?.Length > FieldLengthConstants.Country)
            return new ResponseModel<object>(400, "Country is too long.");

        return await offices.UpdateOfficeAsync(request, cancellationToken);
    }
}
