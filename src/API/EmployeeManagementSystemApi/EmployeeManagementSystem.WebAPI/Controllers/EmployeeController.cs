using System.Text;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.BusinessLogic.Features.Salaries;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ApiControllerBase
{
    private string Username => User.Identity!.Name!;

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateEmployee(
        [FromBody] CreateEmployeeRequest request, [FromServices] CreateEmployeeHandler handler,
        CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, Username, cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<EmployeeModel>>> GetEmployee([FromQuery] string? searchTerm,
        [FromServices] GetEmployeeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(searchTerm, cancellationToken));

    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<PagedResponse<EmployeeModel>>>> GetEmployees(
        [FromQuery] GetEmployeesRequest request, [FromServices] GetEmployeesHandler handler,
        CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> ExportEmployees([FromQuery] ExportEmployeesRequest request,
        [FromServices] ExportEmployeesHandler handler, CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(request, cancellationToken);
        if (response is not { Status: 200, Data: { } csv })
            return Reply(response);

        var bytes = Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"employees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("audit-log")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<AuditLogEntry>>>> GetEmployeeAuditLog(
        [FromQuery] Guid employeeId, [FromServices] GetEmployeeAuditLogHandler handler,
        CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(employeeId, cancellationToken));

    [HttpGet("audit-log/all")]
    public async Task<ActionResult<ResponseModel<PagedResponse<GlobalAuditLogEntry>>>> GetAllEmployeeAuditLog(
        [FromServices] GetAllEmployeeAuditLogHandler handler,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Reply(await handler.HandleAsync(pageNumber, pageSize, cancellationToken));

    [HttpGet("insights")]
    public async Task<ActionResult<ResponseModel<EmployeeInsightsModel>>> GetEmployeeInsights(
        [FromServices] GetEmployeeInsightsHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateEmployee([FromBody] UpdateEmployeeRequest request,
        [FromServices] UpdateEmployeeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, Username, cancellationToken));

    [HttpPatch("deactivate")]
    public async Task<ActionResult<ResponseModel<object>>> DeactivateEmployee([FromQuery] Guid employeeId,
        [FromServices] DeactivateEmployeeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(employeeId, Username, cancellationToken));

    [HttpPatch("reactivate")]
    public async Task<ActionResult<ResponseModel<object>>> ReactivateEmployee([FromQuery] Guid employeeId,
        [FromServices] ReactivateEmployeeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(employeeId, Username, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteEmployee([FromQuery] Guid employeeId,
        [FromServices] DeleteEmployeeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(employeeId, Username, cancellationToken));

    [HttpGet("salary-history")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<SalaryModel>>>> GetEmployeeSalaryHistory(
        [FromQuery] Guid employeeId, [FromServices] GetEmployeeSalaryHistoryHandler handler,
        CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(employeeId, cancellationToken));

    [HttpPost("salary-history")]
    public async Task<ActionResult<ResponseModel<object>>> CreateEmployeeSalary([FromBody] CreateSalaryRequest request,
        [FromServices] CreateEmployeeSalaryHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, Username, cancellationToken));

    [Authorize(Roles = "1801")]
    [HttpDelete("audit-log/all")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteAllEmployeeAuditLog(
        [FromServices] DeleteAllEmployeeAuditLogHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(cancellationToken));
}
