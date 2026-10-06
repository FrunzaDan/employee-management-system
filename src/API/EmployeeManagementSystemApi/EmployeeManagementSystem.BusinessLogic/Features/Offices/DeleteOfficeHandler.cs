using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;

namespace EmployeeManagementSystem.BusinessLogic.Features.Offices;

public class DeleteOfficeHandler(IOfficeRepository offices)
{
    public async Task<ResponseModel<object>> HandleAsync(Guid officeId,
        CancellationToken cancellationToken = default)
    {
        if (officeId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid office ID is required.");

        return await offices.DeleteOfficeAsync(officeId, cancellationToken);
    }
}
