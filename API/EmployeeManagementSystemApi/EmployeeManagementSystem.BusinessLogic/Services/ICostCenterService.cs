using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface ICostCenterService
{
    Task<ResponseModel<object>> CreateCostCenter(CostCenterModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetCostCenter(Guid guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetCostCenters(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> EditCostCenter(CostCenterModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteCostCenter(Guid guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetEmployeesByCostCenter(Guid guid, CancellationToken cancellationToken = default);
}
