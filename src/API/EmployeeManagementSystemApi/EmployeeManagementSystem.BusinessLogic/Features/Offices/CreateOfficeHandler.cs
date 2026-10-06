using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Constants;

namespace EmployeeManagementSystem.BusinessLogic.Features.Offices;

public class CreateOfficeHandler(IOfficeRepository offices)
{
    public async Task<ResponseModel<Guid?>> HandleAsync(CreateOfficeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return new ResponseModel<Guid?>(400, "Office name is required.");
        if (request.Name.Length > FieldLengthConstants.OfficeName)
            return new ResponseModel<Guid?>(400, "Office name is too long.");
        if (request.City?.Length > FieldLengthConstants.City)
            return new ResponseModel<Guid?>(400, "City is too long.");
        if (request.Country?.Length > FieldLengthConstants.Country)
            return new ResponseModel<Guid?>(400, "Country is too long.");

        return await offices.CreateOfficeAsync(request, cancellationToken);
    }
}
