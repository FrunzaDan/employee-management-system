using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Offices;

public class GetOfficeHandler(IOfficeRepository offices)
{
    public async Task<ResponseModel<OfficeModel>> HandleAsync(Guid officeId,
        CancellationToken cancellationToken = default)
    {
        if (officeId == Guid.Empty)
            return new ResponseModel<OfficeModel>(400, "A valid office ID is required.");

        return await offices.GetOfficeAsync(officeId, cancellationToken);
    }
}
