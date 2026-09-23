using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OfficeController(IOfficeService officeService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<IActionResult> GetOffices(CancellationToken cancellationToken)
    {
        var response = await officeService.GetOffices(cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetOffice([FromQuery] Guid guid, CancellationToken cancellationToken)
    {
        var response = await officeService.GetOffice(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOffice([FromBody] OfficeModel request, CancellationToken cancellationToken)
    {
        var response = await officeService.CreateOffice(request, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPatch("edit")]
    public async Task<IActionResult> EditOffice([FromBody] OfficeModel request, CancellationToken cancellationToken)
    {
        var response = await officeService.EditOffice(request, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteOffice([FromQuery] Guid guid, CancellationToken cancellationToken)
    {
        var response = await officeService.DeleteOffice(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeesByOffice([FromQuery] Guid guid, CancellationToken cancellationToken)
    {
        var response = await officeService.GetEmployeesByOffice(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }
}
