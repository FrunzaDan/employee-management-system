using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Departments;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ApiControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<DepartmentModel>>>> GetDepartments(
        [FromServices] GetDepartmentsHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<DepartmentModel>>> GetDepartment([FromQuery] Guid departmentId,
        [FromServices] GetDepartmentHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(departmentId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateDepartment([FromBody] CreateDepartmentRequest request,
        [FromServices] CreateDepartmentHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateDepartment([FromBody] UpdateDepartmentRequest request,
        [FromServices] UpdateDepartmentHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteDepartment([FromQuery] Guid departmentId,
        [FromServices] DeleteDepartmentHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(departmentId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByDepartment(
        [FromQuery] Guid departmentId, [FromServices] GetEmployeesByDepartmentHandler handler,
        CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(departmentId, cancellationToken));
}
