using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IOfficeService
{
    Task<ResponseModel<Guid?>> CreateOfficeAsync(CreateOfficeRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<OfficeModel>> GetOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOfficesAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateOfficeAsync(UpdateOfficeRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
}
