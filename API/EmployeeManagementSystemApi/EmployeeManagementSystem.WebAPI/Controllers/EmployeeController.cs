using System.Text;
using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController(IEmployeeService employeeService) : ControllerBase
{
    // Every [Authorize]-gated request has a verified JWT with ClaimTypes.Name set to the
    // employer ID (see JwtCreation.BuildTokenDescriptor) — never null/empty in practice.
    private string EmployerId => User.Identity!.Name!;

    [HttpPost("register")]
    public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeModel employeeRqst,
        CancellationToken cancellationToken)
    {
        var response = await employeeService.RegisterEmployee(employeeRqst, EmployerId, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetEmployee([FromQuery] string searchVariable,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(searchVariable))
            return BadRequest(new { Message = "Search variable cannot be null or empty." });

        var getEmployeeRqst = new GetEmployeeRequest
        {
            SearchVariable = searchVariable
        };
        var response = await employeeService.GetEmployee(getEmployeeRqst, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetEmployees([FromQuery] GetEmployeesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await employeeService.GetEmployees(request, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportEmployees([FromQuery] ExportEmployeesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await employeeService.GetEmployeesForExport(request, cancellationToken);
        if (response.Status != 200 || response.Data is not string csv)
            return StatusCode(response.Status ?? 200, response);

        var bytes = Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"employees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("auditLog")]
    public async Task<IActionResult> GetEmployeeAuditLog([FromQuery] string employeeGuid,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(employeeGuid))
            return BadRequest(new { Message = "Employee GUID cannot be null or empty." });

        var response = await employeeService.GetEmployeeAuditLog(employeeGuid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("auditLog/all")]
    public async Task<IActionResult> GetAllEmployeeAuditLog([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await employeeService.GetAllEmployeeAuditLog(pageNumber, pageSize, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPatch("edit")]
    public async Task<IActionResult> EditEmployee([FromBody] EmployeeModel editEmployeeRqst,
        CancellationToken cancellationToken)
    {
        var response = await employeeService.EditEmployee(editEmployeeRqst, EmployerId, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPatch("deactivate")]
    public async Task<IActionResult> DeactivateEmployee([FromQuery] string employeeGuid,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(employeeGuid))
            return BadRequest(new { Message = "Employee GUID cannot be null or empty." });

        var response = await employeeService.DeactivateEmployee(employeeGuid, EmployerId, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPatch("reactivate")]
    public async Task<IActionResult> ReactivateEmployee([FromQuery] string employeeGuid,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(employeeGuid))
            return BadRequest(new { Message = "Employee GUID cannot be null or empty." });

        var response = await employeeService.ReactivateEmployee(employeeGuid, EmployerId, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteEmployee([FromQuery] string employeeGuid,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(employeeGuid))
            return BadRequest(new { Message = "Employee GUID cannot be null or empty." });

        var response = await employeeService.DeleteEmployee(employeeGuid, EmployerId, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpGet("salaryHistory")]
    public async Task<IActionResult> GetEmployeeSalaryHistory([FromQuery] string employeeGuid,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(employeeGuid))
            return BadRequest(new { Message = "Employee GUID cannot be null or empty." });

        var response = await employeeService.GetEmployeeSalaryHistory(employeeGuid, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    [HttpPost("salaryHistory")]
    public async Task<IActionResult> AddEmployeeSalary([FromBody] SalaryHistoryEntry request,
        CancellationToken cancellationToken)
    {
        var response = await employeeService.AddEmployeeSalary(request, EmployerId, cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }

    // Explicit role check (not just the class-level [Authorize]) on top of a destructive,
    // untargeted action — wipes every audit row for every employee in one call. Today this
    // is a no-op in practice (1801 is the only employer_role that exists), but it stops a
    // future second role from silently inheriting access to this action.
    [Authorize(Roles = "1801")]
    [HttpDelete("auditLog/all")]
    public async Task<IActionResult> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken)
    {
        var response = await employeeService.DeleteAllEmployeeAuditLog(cancellationToken);
        return StatusCode(response.Status ?? 200, response);
    }
}
