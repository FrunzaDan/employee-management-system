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
    private string Username => User.Identity!.Name!;

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateEmployee(
        [FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken) =>
        Reply(await employeeService.CreateEmployeeAsync(request, Username, cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<EmployeeModel>>> GetEmployee([FromQuery] string? searchTerm,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployeeAsync(searchTerm, cancellationToken));

    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<PagedResponse<EmployeeModel>>>> GetEmployees(
        [FromQuery] GetEmployeesRequest request, CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployeesAsync(request, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> ExportEmployees([FromQuery] ExportEmployeesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await employeeService.GetEmployeesForExportAsync(request, cancellationToken);
        if (response is not { Status: 200, Data: { } csv })
            return Reply(response);

        var bytes = Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"employees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("audit-log")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<AuditLogEntry>>>> GetEmployeeAuditLog(
        [FromQuery] Guid employeeId, CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployeeAuditLogAsync(employeeId, cancellationToken));

    [HttpGet("audit-log/all")]
    public async Task<ActionResult<ResponseModel<PagedResponse<GlobalAuditLogEntry>>>> GetAllEmployeeAuditLog(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Reply(await employeeService.GetAllEmployeeAuditLogAsync(pageNumber, pageSize, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateEmployee([FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.UpdateEmployeeAsync(request, Username, cancellationToken));

    [HttpPatch("deactivate")]
    public async Task<ActionResult<ResponseModel<object>>> DeactivateEmployee([FromQuery] Guid employeeId,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.DeactivateEmployeeAsync(employeeId, Username, cancellationToken));

    [HttpPatch("reactivate")]
    public async Task<ActionResult<ResponseModel<object>>> ReactivateEmployee([FromQuery] Guid employeeId,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.ReactivateEmployeeAsync(employeeId, Username, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteEmployee([FromQuery] Guid employeeId,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.DeleteEmployeeAsync(employeeId, Username, cancellationToken));

    [HttpGet("salary-history")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<SalaryModel>>>> GetEmployeeSalaryHistory(
        [FromQuery] Guid employeeId, CancellationToken cancellationToken) =>
        Reply(await employeeService.GetEmployeeSalaryHistoryAsync(employeeId, cancellationToken));

    [HttpPost("salary-history")]
    public async Task<ActionResult<ResponseModel<object>>> CreateEmployeeSalary([FromBody] CreateSalaryRequest request,
        CancellationToken cancellationToken) =>
        Reply(await employeeService.CreateEmployeeSalaryAsync(request, Username, cancellationToken));

    [Authorize(Roles = "1801")]
    [HttpDelete("audit-log/all")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteAllEmployeeAuditLog(
        CancellationToken cancellationToken) =>
        Reply(await employeeService.DeleteAllEmployeeAuditLogAsync(cancellationToken));
}
