using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Abstractions;

/// <summary>Cost center persistence. Implemented by DataAccess (stored procedures).</summary>
public interface ICostCenterRepository
{
    Task<ResponseModel<Guid?>> CreateCostCenterAsync(CreateCostCenterRequest costCenter, CancellationToken cancellationToken = default);
    Task<ResponseModel<CostCenterModel>> GetCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCentersAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateCostCenterAsync(UpdateCostCenterRequest costCenter, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
}
