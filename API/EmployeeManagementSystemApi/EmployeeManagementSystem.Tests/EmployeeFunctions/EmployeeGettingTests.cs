using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeGettingTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEmployeeFunction_RejectsAnEmptySearchVariable_WithoutTouchingTheDb(string? searchVariable)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeeRequest { SearchVariable = searchVariable };

        var result = await getting.GetEmployeeFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(404, result.Status);
        dbUtils.Verify(d => d.GetEmployee(It.IsAny<GetEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeeFunction_RejectsASearchVariableThatIsNeitherGuidMsisdnNorEmail()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeeRequest { SearchVariable = "not-a-valid-search-term" };

        var result = await getting.GetEmployeeFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(404, result.Status);
        dbUtils.Verify(d => d.GetEmployee(It.IsAny<GetEmployeeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", EmployeeSearchOption.Guid)]
    [InlineData("123456789", EmployeeSearchOption.Msisdn)]
    [InlineData("dan@example.com", EmployeeSearchOption.Email)]
    public async Task GetEmployeeFunction_DetectsTheSearchOptionFromTheSearchVariableShape(string searchVariable,
        EmployeeSearchOption expectedSearchOption)
    {
        var dbUtils = new Mock<IDbUtils>();
        GetEmployeeRequest? capturedRequest = null;
        dbUtils.Setup(d => d.GetEmployee(It.IsAny<GetEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeeRequest, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new ResponseModel<object>(200, "Success!"));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeeRequest { SearchVariable = searchVariable };

        await getting.GetEmployeeFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(expectedSearchOption, capturedRequest!.SearchOption);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetEmployeesFunction_RejectsAnInvalidPageNumber_WithoutTouchingTheDb(int pageNumber)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { PageNumber = pageNumber, PageSize = 10 };

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetEmployeesFunction_RejectsAnInvalidPageSize_WithoutTouchingTheDb(int pageSize)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { PageNumber = 1, PageSize = pageSize };

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesFunction_RejectsAnInvalidSortColumn_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SortColumn = "not-a-real-column" };

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesFunction_RejectsAnInvalidSortDirection_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SortDirection = "sideways" };

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("NAME", "DESC", "name", "desc")]
    [InlineData(" Email ", " Asc ", "email", "asc")]
    public async Task GetEmployeesFunction_NormalizesSortColumnAndDirectionToLowercase(
        string sortColumn, string sortDirection, string expectedColumn, string expectedDirection)
    {
        var dbUtils = new Mock<IDbUtils>();
        GetEmployeesRequest? captured = null;
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<object>(200, "Success!"));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SortColumn = sortColumn, SortDirection = sortDirection };

        await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(expectedColumn, captured!.SortColumn);
        Assert.Equal(expectedDirection, captured.SortDirection);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEmployeesFunction_TreatsABlankSearchTermAsNoSearch(string? searchTerm)
    {
        var dbUtils = new Mock<IDbUtils>();
        GetEmployeesRequest? captured = null;
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<object>(200, "Success!"));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SearchTerm = searchTerm };

        await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Null(captured!.SearchTerm);
    }

    [Fact]
    public async Task GetEmployeesFunction_ReturnsWhateverTheDbLayerReturns()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<object>(200, "Success!",
            new PagedResponse<EmployeeModel>(new List<EmployeeModel>(), 0, 1, 10));
        var request = new GetEmployeesRequest { PageNumber = 1, PageSize = 10 };
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_RejectsAnInvalidSortColumn_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new ExportEmployeesRequest { SortColumn = "not-a-real-column" };

        var result = await getting.GetEmployeesForExportFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_RejectsAnInvalidSortDirection_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new ExportEmployeesRequest { SortDirection = "sideways" };

        var result = await getting.GetEmployeesForExportFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_IgnoresPagingAndRequestsTheFullCappedResultInOneCall()
    {
        var dbUtils = new Mock<IDbUtils>();
        GetEmployeesRequest? captured = null;
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<object>(200, "Success!",
                new PagedResponse<EmployeeModel>(new List<EmployeeModel>(), 0, 1, 5000)));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new ExportEmployeesRequest { SearchTerm = " dan ", SortColumn = " EMAIL ", SortDirection = " DESC " };

        await getting.GetEmployeesForExportFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(1, captured!.PageNumber);
        Assert.Equal(5000, captured.PageSize);
        Assert.Equal("dan", captured.SearchTerm);
        Assert.Equal("email", captured.SortColumn);
        Assert.Equal("desc", captured.SortDirection);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_ReturnsCsvBuiltFromTheDbLayersPagedItems()
    {
        var dbUtils = new Mock<IDbUtils>();
        var employee = new EmployeeModel { Guid = Guid.NewGuid(), FirstName = "Dan", LastName = "Frunza" };
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(200, "Success!",
                new PagedResponse<EmployeeModel>([employee], 1, 1, 5000)));
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesForExportFunction(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        var csv = Assert.IsType<string>(result.Data);
        Assert.Contains("Dan", csv);
        Assert.Contains("Frunza", csv);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_PassesThroughADbLayerFailureUnchanged()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<object>(500, "Something went wrong.");
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesForExportFunction(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAllAuditLogFunction_RejectsAnInvalidPageNumber_WithoutTouchingTheDb(int pageNumber)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetAllAuditLogFunction(pageNumber, 10, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetAllEmployeeAuditLog(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetAllAuditLogFunction_RejectsAnInvalidPageSize_WithoutTouchingTheDb(int pageSize)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetAllAuditLogFunction(1, pageSize, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetAllEmployeeAuditLog(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAuditLogFunction_ReturnsWhateverTheDbLayerReturns()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<object>(200, "Success!",
            new PagedResponse<GlobalAuditLogEntry>(new List<GlobalAuditLogEntry>(), 0, 1, 10));
        dbUtils.Setup(d => d.GetAllEmployeeAuditLog(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetAllAuditLogFunction(1, 10, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.GetAllEmployeeAuditLog(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
