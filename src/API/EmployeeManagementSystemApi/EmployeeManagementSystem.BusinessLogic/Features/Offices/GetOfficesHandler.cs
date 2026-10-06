using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Offices;

public class GetOfficesHandler(IOfficeRepository offices)
{
    public async Task<ResponseModel<IReadOnlyList<OfficeModel>>> HandleAsync(
        CancellationToken cancellationToken = default) =>
        await offices.GetOfficesAsync(cancellationToken);
}
