using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class OfficeService(OfficeFunctions officeFunctions) : IOfficeService
{
    public async Task<ResponseModel<Guid?>> CreateOffice(CreateOfficeRequest request,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.CreateOfficeFunction(request, cancellationToken);

    public async Task<ResponseModel<OfficeModel>> GetOffice(Guid officeId,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetOfficeFunction(officeId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOffices(
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetOfficesFunction(cancellationToken);

    public async Task<ResponseModel<object>> UpdateOffice(UpdateOfficeRequest request,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.UpdateOfficeFunction(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteOffice(Guid officeId,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.DeleteOfficeFunction(officeId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOffice(Guid officeId,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetEmployeesByOfficeFunction(officeId, cancellationToken);
}
