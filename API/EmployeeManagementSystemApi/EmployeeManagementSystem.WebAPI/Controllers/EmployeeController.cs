using System.Text;
using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController(IEmployeeService employeeService) : ApiControllerBase
{
    // Every [Authorize]-gated request has a verified JWT with ClaimTypes.Name set to the
    // employer's username (see JwtCreation.BuildTokenDescriptor) — never null/empty in practice.
    private string Username => User.Identity!.Name!;

    // ID query parameters are typed Guid: a malformed value is rejected by model binding
    // (400 ValidationProblemDetails, from [ApiController]), and a missing one binds
    // to Guid.Empty, which the business logic rejects with its own 400.

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateEmployee(
        [FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken) =>
        Reply(await employeeService.CreateEmployee(request, Username, cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<EmployeeModel>>> GetEmployee([FromQuery] string? searchTerm,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployee(searchTerm, cancellationToken));

    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<PagedResponse<EmployeeModel>>>> GetEmployees(
        [FromQuery] GetEmployeesRequest request, CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployees(request, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> ExportEmployees([FromQuery] ExportEmployeesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await employeeService.GetEmployeesForExport(request, cancellationToken);
        if (response is not { Status: 200, Data: { } csv })
            return Reply(response);

        var bytes = Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"employees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("audit-log")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<AuditLogEntry>>>> GetEmployeeAuditLog(
        [FromQuery] Guid employeeId, CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployeeAuditLog(employeeId, cancellationToken));

    [HttpGet("audit-log/all")]
    public async Task<ActionResult<ResponseModel<PagedResponse<GlobalAuditLogEntry>>>> GetAllEmployeeAuditLog(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Reply(await employeeService.GetAllEmployeeAuditLog(pageNumber, pageSize, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateEmployee([FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.UpdateEmployee(request, Username, cancellationToken));

    [HttpPatch("deactivate")]
    public async Task<ActionResult<ResponseModel<object>>> DeactivateEmployee([FromQuery] Guid employeeId,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.DeactivateEmployee(employeeId, Username, cancellationToken));

    [HttpPatch("reactivate")]
    public async Task<ActionResult<ResponseModel<object>>> ReactivateEmployee([FromQuery] Guid employeeId,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.ReactivateEmployee(employeeId, Username, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteEmployee([FromQuery] Guid employeeId,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.DeleteEmployee(employeeId, Username, cancellationToken));

    [HttpGet("salary-history")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<SalaryModel>>>> GetEmployeeSalaryHistory(
        [FromQuery] Guid employeeId, CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployeeSalaryHistory(employeeId, cancellationToken));

    [HttpPost("salary-history")]
    public async Task<ActionResult<ResponseModel<object>>> CreateEmployeeSalary([FromBody] CreateSalaryRequest request,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.CreateEmployeeSalary(request, Username, cancellationToken));

    // Explicit role check (not just the class-level [Authorize]) on top of a destructive,
    // untargeted action — wipes every audit row for every employee in one call. Today this
    // is a no-op in practice (1801 is the only Employer.RoleCode that exists), but it stops a
    // future second role from silently inheriting access to this action.
    [Authorize(Roles = "1801")]
    [HttpDelete("audit-log/all")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteAllEmployeeAuditLog(
        CancellationToken cancellationToken) =>
        Reply(await employeeService.DeleteAllEmployeeAuditLog(cancellationToken));
}
