using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class ExportEmployeesHandlerTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private static EmployeeModel MakeEmployee() => new()
    {
        EmployeeId = EmployeeId,
        FirstName = "Dan",
        LastName = "Frunza",
        Email = "dan@example.com",
        PhoneNumber = "123456789",
        Gender = Gender.Male,
        Status = EmployeeStatus.Active,
        AccountCreatedAt = DateTime.UtcNow,
        LastInteractionAt = DateTime.UtcNow,
        Address = new AddressModel
        {
            Country = "Romania",
            County = "Cluj",
            City = "Cluj-Napoca",
            PostalCode = "400001",
            Street = "Main",
            StreetNumber = "1"
        }
    };

    [Fact]
    public async Task GetEmployeesForExportAsync_RejectsAnUndefinedSortColumn_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new ExportEmployeesHandler(employees.Object);
        var request = new ExportEmployeesRequest { SortColumn = (EmployeeSortColumn)7 };

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_IgnoresPagingAndRequestsTheFullCappedResultInOneCall()
    {
        var employees = new Mock<IEmployeeRepository>();
        GetEmployeesRequest? captured = null;
        employees.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([], 0, 1, 5000)));
        var handler = new ExportEmployeesHandler(employees.Object);
        var request = new ExportEmployeesRequest
        {
            SearchTerm = " dan ",
            SortColumn = EmployeeSortColumn.Email,
            SortDirection = SortDirection.Desc
        };

        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(1, captured!.PageNumber);
        Assert.Equal(5000, captured.PageSize);
        Assert.Equal("dan", captured.SearchTerm);
        Assert.Equal(EmployeeSortColumn.Email, captured.SortColumn);
        Assert.Equal(SortDirection.Desc, captured.SortDirection);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_PassesTheOrgFiltersThrough()
    {
        var employees = new Mock<IEmployeeRepository>();
        GetEmployeesRequest? captured = null;
        employees.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([], 0, 1, 5000)));
        var handler = new ExportEmployeesHandler(employees.Object);
        var request = new ExportEmployeesRequest
        {
            OfficeId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            CostCenterId = Guid.NewGuid()
        };

        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(request.OfficeId, captured!.OfficeId);
        Assert.Equal(request.DepartmentId, captured.DepartmentId);
        Assert.Equal(request.CostCenterId, captured.CostCenterId);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_ReturnsCsvBuiltFromTheDbLayersPagedItems()
    {
        var employees = new Mock<IEmployeeRepository>();
        employees.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([MakeEmployee()], 1, 1, 5000)));
        var handler = new ExportEmployeesHandler(employees.Object);

        var result = await handler.HandleAsync(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.Contains("Dan", result.Data);
        Assert.Contains("Frunza", result.Data);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_RejectsAResultLargerThanTheExportCap()
    {
        var employees = new Mock<IEmployeeRepository>();
        employees.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([MakeEmployee()], 5001, 1, 5000)));
        var handler = new ExportEmployeesHandler(employees.Object);

        var result = await handler.HandleAsync(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains("limited to 5000", result.ResponseMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_PassesThroughADbLayerFailureUnchanged()
    {
        var employees = new Mock<IEmployeeRepository>();
        employees.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(500, "Something went wrong."));
        var handler = new ExportEmployeesHandler(employees.Object);

        var result = await handler.HandleAsync(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(500, result.Status);
        Assert.Equal("Something went wrong.", result.ResponseMessage);
        Assert.Null(result.Data);
    }
}
