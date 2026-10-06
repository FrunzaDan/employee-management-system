using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.CostCenters;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CostCenterController : ApiControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<CostCenterModel>>>> GetCostCenters(
        [FromServices] GetCostCentersHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<CostCenterModel>>> GetCostCenter([FromQuery] Guid costCenterId,
        [FromServices] GetCostCenterHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(costCenterId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateCostCenter([FromBody] CreateCostCenterRequest request,
        [FromServices] CreateCostCenterHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateCostCenter([FromBody] UpdateCostCenterRequest request,
        [FromServices] UpdateCostCenterHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteCostCenter([FromQuery] Guid costCenterId,
        [FromServices] DeleteCostCenterHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(costCenterId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByCostCenter(
        [FromQuery] Guid costCenterId, [FromServices] GetEmployeesByCostCenterHandler handler,
        CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(costCenterId, cancellationToken));
}
