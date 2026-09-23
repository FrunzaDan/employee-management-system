using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IOfficeService
{
    Task<ResponseModel<Guid?>> CreateOffice(CreateOfficeRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<OfficeModel>> GetOffice(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOffices(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> EditOffice(UpdateOfficeRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteOffice(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOffice(Guid officeId, CancellationToken cancellationToken = default);
}
