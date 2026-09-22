using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface ICostCenterService
{
    Task<ResponseModel<object>> CreateCostCenter(CostCenterModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetCostCenter(string guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetCostCenters(CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> EditCostCenter(CostCenterModel request, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> DeleteCostCenter(string guid, CancellationToken cancellationToken = default);
    Task<ResponseModel<object>> GetEmployeesByCostCenter(string guid, CancellationToken cancellationToken = default);
}
