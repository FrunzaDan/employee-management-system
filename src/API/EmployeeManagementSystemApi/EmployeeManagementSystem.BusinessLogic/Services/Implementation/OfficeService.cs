using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class OfficeService(OfficeFunctions officeFunctions) : IOfficeService
{
    public async Task<ResponseModel<Guid?>> CreateOfficeAsync(CreateOfficeRequest request,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.CreateOfficeAsync(request, cancellationToken);

    public async Task<ResponseModel<OfficeModel>> GetOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetOfficeAsync(officeId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOfficesAsync(
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetOfficesAsync(cancellationToken);

    public async Task<ResponseModel<object>> UpdateOfficeAsync(UpdateOfficeRequest request,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.UpdateOfficeAsync(request, cancellationToken);

    public async Task<ResponseModel<object>> DeleteOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.DeleteOfficeAsync(officeId, cancellationToken);

    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        await officeFunctions.GetEmployeesByOfficeAsync(officeId, cancellationToken);
}
