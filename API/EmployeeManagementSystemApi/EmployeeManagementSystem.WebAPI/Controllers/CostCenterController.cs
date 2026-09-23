using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CostCenterController(ICostCenterService costCenterService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<CostCenterModel>>>> GetCostCenters(
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.GetCostCenters(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<CostCenterModel>>> GetCostCenter([FromQuery] Guid costCenterId,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.GetCostCenter(costCenterId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateCostCenter([FromBody] CreateCostCenterRequest request,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.CreateCostCenter(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateCostCenter([FromBody] UpdateCostCenterRequest request,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.UpdateCostCenter(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteCostCenter([FromQuery] Guid costCenterId,
        CancellationToken cancellationToken) =>
        Reply(await costCenterService.DeleteCostCenter(costCenterId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByCostCenter(
        [FromQuery] Guid costCenterId, CancellationToken cancellationToken) =>
        Reply(await costCenterService.GetEmployeesByCostCenter(costCenterId, cancellationToken));

    // The envelope's Status is the HTTP status to reply with.
    private ObjectResult Reply<T>(ResponseModel<T> response) => StatusCode(response.Status, response);
}
