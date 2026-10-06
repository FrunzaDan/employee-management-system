using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Abstractions;

/// <summary>Office persistence. Implemented by DataAccess (stored procedures).</summary>
public interface IOfficeRepository
{
    Task<ResponseModel<Guid?>> CreateOfficeAsync(CreateOfficeRequest office, CancellationToken cancellationToken = default);
    Task<ResponseModel<OfficeModel>> GetOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOfficesAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateOfficeAsync(UpdateOfficeRequest office, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
}
