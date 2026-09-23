using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface ICostCenterService
{
    Task<ResponseModel<Guid?>> CreateCostCenter(CreateCostCenterRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<CostCenterModel>> GetCostCenter(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCenters(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> EditCostCenter(UpdateCostCenterRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteCostCenter(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenter(Guid costCenterId, CancellationToken cancellationToken = default);
}
