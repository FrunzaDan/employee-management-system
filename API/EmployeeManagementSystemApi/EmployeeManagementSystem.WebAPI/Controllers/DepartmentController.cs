using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController(IDepartmentService departmentService) : ApiControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<DepartmentModel>>>> GetDepartments(
        CancellationToken cancellationToken) =>
        Reply(await departmentService.GetDepartmentsAsync(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<DepartmentModel>>> GetDepartment([FromQuery] Guid departmentId,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.GetDepartmentAsync(departmentId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateDepartment([FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.CreateDepartmentAsync(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateDepartment([FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.UpdateDepartmentAsync(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteDepartment([FromQuery] Guid departmentId,
        CancellationToken cancellationToken) =>
        Reply(await departmentService.DeleteDepartmentAsync(departmentId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByDepartment(
        [FromQuery] Guid departmentId, CancellationToken cancellationToken) =>
        Reply(await departmentService.GetEmployeesByDepartmentAsync(departmentId, cancellationToken));
}
