using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeGettingTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private static readonly Guid ProductId = Guid.Parse("2432276c-4ef0-4e50-abc5-8b5f82297844");

    private static EmployeeModel MakeEmployee() => new()
    {
        EmployeeId = EmployeeId,
        FirstName = "Dan",
        LastName = "Frunza",
        Email = "dan@example.com",
        PhoneNumber = "123456789",
        Gender = Gender.Male,
        Status = EmployeeStatus.Active,
        CreatedAt = DateTime.UtcNow,
        LastInteractionAt = DateTime.UtcNow,
        Address = new AddressModel
        {
            Country = "Romania", County = "Cluj", City = "Cluj-Napoca", PostalCode = "400001", Street = "Main", StreetNumber = "1"
        }
    };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEmployeeFunction_RejectsAnEmptySearchVariable_WithoutTouchingTheDb(string? searchTerm)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeeFunction(searchTerm, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployee(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeeFunction_RejectsASearchVariableThatIsNeitherIdPhoneNumberNorEmail()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeeFunction("not-a-valid-search-term", TestContext.Current.CancellationToken);

        Assert.Equal(404, result.Status);
        dbUtils.Verify(d => d.GetEmployee(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    // Every GUID spelling resolves to the same value — braces and case no longer matter,
    // since the key is compared as a UNIQUEIDENTIFIER, not as text.
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    [InlineData("{3FA85F64-5717-4562-B3FC-2C963F66AFA6}")]
    [InlineData("3fa85f6457174562b3fc2c963f66afa6")]
    public async Task GetEmployeeFunction_LooksUpByEmployeeId_ForAnyGuidSpelling(string searchTerm)
    {
        var captured = await CaptureLookup(searchTerm);

        Assert.Equal(new EmployeeLookup(EmployeeId: EmployeeId), captured);
    }

    [Fact]
    public async Task GetEmployeeFunction_LooksUpByPhoneNumber_ForADigitsOnlySearchTerm()
    {
        Assert.Equal(new EmployeeLookup(PhoneNumber: "123456789"), await CaptureLookup("123456789"));
    }

    [Fact]
    public async Task GetEmployeeFunction_LooksUpByEmail_ForAnEmailShapedSearchVariable()
    {
        Assert.Equal(new EmployeeLookup(Email: "dan@example.com"), await CaptureLookup(" dan@example.com "));
    }

    private static async Task<EmployeeLookup?> CaptureLookup(string searchTerm)
    {
        var dbUtils = new Mock<IDbUtils>();
        EmployeeLookup? captured = null;
        dbUtils.Setup(d => d.GetEmployee(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()))
            .Callback<EmployeeLookup, CancellationToken>((l, _) => captured = l)
            .ReturnsAsync(new ResponseModel<EmployeeModel>(200, "Employee found.", MakeEmployee()));
        var getting = new EmployeeGetting(dbUtils.Object);

        await getting.GetEmployeeFunction(searchTerm, TestContext.Current.CancellationToken);

        return captured;
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
    public async Task GetEmployeesFunction_RejectsAnUndefinedSortColumn_WithoutTouchingTheDb()
    {
        // "?sortColumn=7" binds to (EmployeeSortColumn)7 — the enum alone doesn't stop it.
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SortColumn = (EmployeeSortColumn)7 };

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesFunction_RejectsAnUndefinedSortDirection_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SortDirection = (SortDirection)7 };

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!"));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SearchTerm = searchTerm };

        await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Null(captured!.SearchTerm);
    }

    [Fact]
    public async Task GetEmployeesFunction_RejectsASearchTermLongerThanTheProcParameter_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SearchTerm = new string('a', 255) };

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesFunction_ReturnsWhateverTheDbLayerReturns()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
            new PagedResponse<EmployeeModel>([], 0, 1, 10));
        var request = new GetEmployeesRequest { PageNumber = 1, PageSize = 10 };
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesFunction(request, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_RejectsAnUndefinedSortColumn_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new ExportEmployeesRequest { SortColumn = (EmployeeSortColumn)7 };

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
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([], 0, 1, 5000)));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new ExportEmployeesRequest
        {
            SearchTerm = " dan ", SortColumn = EmployeeSortColumn.Email, SortDirection = SortDirection.Desc
        };

        await getting.GetEmployeesForExportFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(1, captured!.PageNumber);
        Assert.Equal(5000, captured.PageSize);
        Assert.Equal("dan", captured.SearchTerm);
        Assert.Equal(EmployeeSortColumn.Email, captured.SortColumn);
        Assert.Equal(SortDirection.Desc, captured.SortDirection);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_ReturnsCsvBuiltFromTheDbLayersPagedItems()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([MakeEmployee()], 1, 1, 5000)));
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesForExportFunction(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.Contains("Dan", result.Data);
        Assert.Contains("Frunza", result.Data);
    }

    [Fact]
    public async Task GetEmployeesForExportFunction_PassesThroughADbLayerFailureUnchanged()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.GetEmployees(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(500, "Something went wrong."));
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesForExportFunction(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(500, result.Status);
        Assert.Equal("Something went wrong.", result.ResponseMessage);
        Assert.Null(result.Data);
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
    public async Task GetEmployeeAuditLogFunction_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeeAuditLogFunction(Guid.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeeAuditLog(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAuditLogFunction_ReturnsWhateverTheDbLayerReturns()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(200, "Success!",
            new PagedResponse<GlobalAuditLogEntry>([], 0, 1, 10));
        dbUtils.Setup(d => d.GetAllEmployeeAuditLog(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetAllAuditLogFunction(1, 10, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.GetAllEmployeeAuditLog(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
