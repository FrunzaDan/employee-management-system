using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Offices;

public class GetEmployeesByOfficeHandler(IOfficeRepository offices)
{
    public async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> HandleAsync(Guid officeId,
        CancellationToken cancellationToken = default)
    {
        if (officeId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(400, "A valid office ID is required.");

        return await offices.GetEmployeesByOfficeAsync(officeId, cancellationToken);
    }
}
