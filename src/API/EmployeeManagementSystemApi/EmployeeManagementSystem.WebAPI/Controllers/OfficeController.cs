using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OfficeController(IOfficeService officeService) : ApiControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<OfficeModel>>>> GetOffices(
        CancellationToken cancellationToken) =>
        Reply(await officeService.GetOfficesAsync(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<OfficeModel>>> GetOffice([FromQuery] Guid officeId,
        CancellationToken cancellationToken) =>
        Reply(await officeService.GetOfficeAsync(officeId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateOffice([FromBody] CreateOfficeRequest request,
        CancellationToken cancellationToken) =>
        Reply(await officeService.CreateOfficeAsync(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateOffice([FromBody] UpdateOfficeRequest request,
        CancellationToken cancellationToken) =>
        Reply(await officeService.UpdateOfficeAsync(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteOffice([FromQuery] Guid officeId,
        CancellationToken cancellationToken) =>
        Reply(await officeService.DeleteOfficeAsync(officeId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByOffice(
        [FromQuery] Guid officeId, CancellationToken cancellationToken) =>
        Reply(await officeService.GetEmployeesByOfficeAsync(officeId, cancellationToken));
}
