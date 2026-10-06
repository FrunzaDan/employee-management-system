using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Offices;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OfficeController : ApiControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<OfficeModel>>>> GetOffices(
        [FromServices] GetOfficesHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(cancellationToken));

    [HttpGet("get")]
    public async Task<ActionResult<ResponseModel<OfficeModel>>> GetOffice([FromQuery] Guid officeId,
        [FromServices] GetOfficeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(officeId, cancellationToken));

    [HttpPost("create")]
    public async Task<ActionResult<ResponseModel<Guid?>>> CreateOffice([FromBody] CreateOfficeRequest request,
        [FromServices] CreateOfficeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, cancellationToken));

    [HttpPatch("update")]
    public async Task<ActionResult<ResponseModel<object>>> UpdateOffice([FromBody] UpdateOfficeRequest request,
        [FromServices] UpdateOfficeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(request, cancellationToken));

    [HttpDelete("delete")]
    public async Task<ActionResult<ResponseModel<object>>> DeleteOffice([FromQuery] Guid officeId,
        [FromServices] DeleteOfficeHandler handler, CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(officeId, cancellationToken));

    [HttpGet("employees")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>>> GetEmployeesByOffice(
        [FromQuery] Guid officeId, [FromServices] GetEmployeesByOfficeHandler handler,
        CancellationToken cancellationToken) =>
        Reply(await handler.HandleAsync(officeId, cancellationToken));
}
