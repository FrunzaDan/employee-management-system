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
    public async Task<ActionResult<ResponseModel<IReadOnlyList<OfficeModel>>>> GetOffices(
        CancellationToken cancellationToken) =>
        Reply(await officeService.GetOffices(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<OfficeModel>>> GetOffice([FromQuery] Guid officeId,
        CancellationToken cancellationToken) =>
        Reply(await officeService.GetOffice(officeId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateOffice([FromBody] CreateOfficeRequest request,
        CancellationToken cancellationToken) =>
        Reply(await officeService.CreateOffice(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateOffice([FromBody] UpdateOfficeRequest request,
        CancellationToken cancellationToken) =>
        Reply(await officeService.UpdateOffice(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteOffice([FromQuery] Guid officeId,
        CancellationToken cancellationToken) =>
        Reply(await officeService.DeleteOffice(officeId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByOffice(
        [FromQuery] Guid officeId, CancellationToken cancellationToken) =>
        Reply(await officeService.GetEmployeesByOffice(officeId, cancellationToken));

    // The envelope's Status is the HTTP status to reply with.
    private ObjectResult Reply<T>(ResponseModel<T> response) => StatusCode(response.Status, response);
}
