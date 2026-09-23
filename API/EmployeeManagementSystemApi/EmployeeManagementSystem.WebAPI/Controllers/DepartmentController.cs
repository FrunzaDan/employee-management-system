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
    public async Task<ActionResult<ResponseModel<IReadOnlyList<DepartmentModel>>>> GetDepartments(
        CancellationToken cancellationToken) =>
        Reply(await departmentService.GetDepartments(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<DepartmentModel>>> GetDepartment([FromQuery] Guid departmentId,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.GetDepartment(departmentId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateDepartment([FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.CreateDepartment(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateDepartment([FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.UpdateDepartment(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteDepartment([FromQuery] Guid departmentId,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.DeleteDepartment(departmentId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByDepartment(
        [FromQuery] Guid departmentId, CancellationToken cancellationToken) =>
        Reply(await departmentService.GetEmployeesByDepartment(departmentId, cancellationToken));

    // The envelope's Status is the HTTP status to reply with.
    private ObjectResult Reply<T>(ResponseModel<T> response) => StatusCode(response.Status, response);
}
