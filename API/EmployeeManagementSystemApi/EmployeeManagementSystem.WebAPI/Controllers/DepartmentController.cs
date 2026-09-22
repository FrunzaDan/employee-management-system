using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController(IDepartmentService departmentService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var response = await departmentService.GetDepartments(cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetDepartment([FromQuery] string guid, CancellationToken cancellationToken)
    {
        var response = await departmentService.GetDepartment(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentModel request,
        CancellationToken cancellationToken)
    {
        var response = await departmentService.CreateDepartment(request, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPatch("edit")]
    public async Task<IActionResult> EditDepartment([FromBody] DepartmentModel request,
        CancellationToken cancellationToken)
    {
        var response = await departmentService.EditDepartment(request, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteDepartment([FromQuery] string guid, CancellationToken cancellationToken)
    {
        var response = await departmentService.DeleteDepartment(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeesByDepartment([FromQuery] string guid,
        CancellationToken cancellationToken)
    {
        var response = await departmentService.GetEmployeesByDepartment(guid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }
}
