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
    public async Task<IActionResult> GetCostCenters(CancellationToken cancellationToken)
    {
        var response = await costCenterService.GetCostCenters(cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetCostCenter([FromQuery] string guid, CancellationToken cancellationToken)
    {
        var response = await costCenterService.GetCostCenter(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCostCenter([FromBody] CostCenterModel request,
        CancellationToken cancellationToken)
    {
        var response = await costCenterService.CreateCostCenter(request, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPatch("edit")]
    public async Task<IActionResult> EditCostCenter([FromBody] CostCenterModel request,
        CancellationToken cancellationToken)
    {
        var response = await costCenterService.EditCostCenter(request, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCostCenter([FromQuery] string guid, CancellationToken cancellationToken)
    {
        var response = await costCenterService.DeleteCostCenter(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeesByCostCenter([FromQuery] string guid,
        CancellationToken cancellationToken)
    {
        var response = await costCenterService.GetEmployeesByCostCenter(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }
}
