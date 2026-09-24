using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.OrgFunctions;

public class OfficeFunctions(IDbUtils dbUtils)
{
    public async Task<ResponseModel<Guid?>> CreateOfficeAsync(CreateOfficeRequest request,
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

        return await dbUtils.CreateOfficeAsync(request, cancellationToken);
    }

    public async Task<ResponseModel<OfficeModel>> GetOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default)
    {
        if (officeId == Guid.Empty)
            return new ResponseModel<OfficeModel>(400, "A valid office ID is required.");

        return await dbUtils.GetOfficeAsync(officeId, cancellationToken);
    }

    public async Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOfficesAsync(
        CancellationToken cancellationToken = default) =>
        await dbUtils.GetOfficesAsync(cancellationToken);

    public async Task<ResponseModel<object>> UpdateOfficeAsync(UpdateOfficeRequest request,
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

        return await dbUtils.UpdateOfficeAsync(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> DeleteOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default)
    {
        if (officeId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid office ID is required.");

        return await dbUtils.DeleteOfficeAsync(officeId, cancellationToken);
    }

    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default)
    {
        if (officeId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(400, "A valid office ID is required.");

        return await dbUtils.GetEmployeesByOfficeAsync(officeId, cancellationToken);
    }
}
