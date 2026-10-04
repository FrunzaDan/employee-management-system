using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CostCenterController(ICostCenterService costCenterService) : ApiControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<CostCenterModel>>>> GetCostCenters(
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.GetCostCentersAsync(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<CostCenterModel>>> GetCostCenter([FromQuery] Guid costCenterId,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.GetCostCenterAsync(costCenterId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateCostCenter([FromBody] CreateCostCenterRequest request,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.CreateCostCenterAsync(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateCostCenter([FromBody] UpdateCostCenterRequest request,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.UpdateCostCenterAsync(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteCostCenter([FromQuery] Guid costCenterId,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.DeleteCostCenterAsync(costCenterId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByCostCenter(
        [FromQuery] Guid costCenterId, CancellationToken cancellationToken) =>
        Reply(await costCenterService.GetEmployeesByCostCenterAsync(costCenterId, cancellationToken));
}
