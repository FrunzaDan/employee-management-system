using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.OrgFunctions;

public class OfficeFunctions(IDbUtils dbUtils)
{
    public async Task<ResponseModel<object>> CreateOfficeFunction(OfficeModel request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OfficeName))
            return new ResponseModel<object>(400, "Office name is required.");
        if (request.OfficeName.Length > FieldLengthConstants.OfficeName)
            return new ResponseModel<object>(400, "Office name is too long.");
        if (!string.IsNullOrEmpty(request.City) && request.City.Length > FieldLengthConstants.City)
            return new ResponseModel<object>(400, "City is too long.");
        if (!string.IsNullOrEmpty(request.Country) && request.Country.Length > FieldLengthConstants.Country)
            return new ResponseModel<object>(400, "Country is too long.");

        // An office's identifier is always generated server-side, same as every other
        // GUID in this app (see database.md's glossary entry).
        request.Guid = SequentialGuid.NewGuid();

        return await dbUtils.CreateOffice(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetOfficeFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid office GUID is required.");

        return await dbUtils.GetOffice(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetOfficesFunction(CancellationToken cancellationToken = default) =>
        await dbUtils.GetOffices(cancellationToken);

    public async Task<ResponseModel<object>> EditOfficeFunction(OfficeModel request,
        CancellationToken cancellationToken = default)
    {
        if (request.Guid is null || request.Guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid office GUID is required.");
        if (!string.IsNullOrEmpty(request.OfficeName) && request.OfficeName.Length > FieldLengthConstants.OfficeName)
            return new ResponseModel<object>(400, "Office name is too long.");
        if (!string.IsNullOrEmpty(request.City) && request.City.Length > FieldLengthConstants.City)
            return new ResponseModel<object>(400, "City is too long.");
        if (!string.IsNullOrEmpty(request.Country) && request.Country.Length > FieldLengthConstants.Country)
            return new ResponseModel<object>(400, "Country is too long.");

        return await dbUtils.EditOffice(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> DeleteOfficeFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid office GUID is required.");

        return await dbUtils.DeleteOffice(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetEmployeesByOfficeFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid office GUID is required.");

        return await dbUtils.GetEmployeesByOffice(guid, cancellationToken);
    }
}
