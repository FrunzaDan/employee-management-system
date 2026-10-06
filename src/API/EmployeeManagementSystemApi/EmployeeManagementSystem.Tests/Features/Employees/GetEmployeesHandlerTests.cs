using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class GetEmployeesHandlerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetEmployeesAsync_RejectsAnInvalidPageNumber_WithoutTouchingTheDb(int pageNumber)
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new GetEmployeesHandler(employees.Object);
        var request = new GetEmployeesRequest { PageNumber = pageNumber, PageSize = 10 };

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetEmployeesAsync_RejectsAnInvalidPageSize_WithoutTouchingTheDb(int pageSize)
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new GetEmployeesHandler(employees.Object);
        var request = new GetEmployeesRequest { PageNumber = 1, PageSize = pageSize };

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesAsync_RejectsAnUndefinedSortColumn_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new GetEmployeesHandler(employees.Object);
        var request = new GetEmployeesRequest { SortColumn = (EmployeeSortColumn)7 };

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesAsync_RejectsAnUndefinedSortDirection_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new GetEmployeesHandler(employees.Object);
        var request = new GetEmployeesRequest { SortDirection = (SortDirection)7 };

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEmployeesAsync_TreatsABlankSearchTermAsNoSearch(string? searchTerm)
    {
        var employees = new Mock<IEmployeeRepository>();
        GetEmployeesRequest? captured = null;
        employees.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!"));
        var handler = new GetEmployeesHandler(employees.Object);
        var request = new GetEmployeesRequest { SearchTerm = searchTerm };

        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Null(captured!.SearchTerm);
    }

    [Fact]
    public async Task GetEmployeesAsync_RejectsASearchTermLongerThanTheProcParameter_WithoutTouchingTheDb()
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new GetEmployeesHandler(employees.Object);
        var request = new GetEmployeesRequest { SearchTerm = new string('a', 255) };

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesAsync_ReturnsWhateverTheDbLayerReturns()
    {
        var employees = new Mock<IEmployeeRepository>();
        var expected = new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
            new PagedResponse<EmployeeModel>([], 0, 1, 10));
        var request = new GetEmployeesRequest { PageNumber = 1, PageSize = 10 };
        employees.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new GetEmployeesHandler(employees.Object);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        employees.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
