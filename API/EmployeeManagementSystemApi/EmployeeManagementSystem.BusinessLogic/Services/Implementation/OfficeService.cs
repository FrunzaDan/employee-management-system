using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class OfficeService(OfficeFunctions officeFunctions) : IOfficeService
{
    public async Task<ResponseModel<object>> CreateOffice(OfficeModel request,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.CreateOfficeFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> GetOffice(string guid,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetOfficeFunction(guid, cancellationToken);

    public async Task<ResponseModel<object>> GetOffices(CancellationToken cancellationToken = default) =>
        await officeFunctions.GetOfficesFunction(cancellationToken);

    public async Task<ResponseModel<object>> EditOffice(OfficeModel request,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.EditOfficeFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteOffice(string guid,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.DeleteOfficeFunction(guid, cancellationToken);

    public async Task<ResponseModel<object>> GetEmployeesByOffice(string guid,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetEmployeesByOfficeFunction(guid, cancellationToken);
}
