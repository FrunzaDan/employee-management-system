using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface ICostCenterService
{
    Task<ResponseModel<Guid?>> CreateCostCenterAsync(CreateCostCenterRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<CostCenterModel>> GetCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCentersAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> UpdateCostCenterAsync(UpdateCostCenterRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
}
