using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IOfficeService
{
    Task<ResponseModel<object>> CreateOffice(OfficeModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetOffice(Guid guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetOffices(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> EditOffice(OfficeModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteOffice(Guid guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetEmployeesByOffice(Guid guid, CancellationToken cancellationToken = default);
}
