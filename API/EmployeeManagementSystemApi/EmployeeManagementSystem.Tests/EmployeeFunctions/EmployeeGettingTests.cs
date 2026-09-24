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
    public async Task GetEmployeeAsync_RejectsAnEmptySearchVariable_WithoutTouchingTheDb(string? searchTerm)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeeAsync(searchTerm, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeeAsync(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeeAsync_RejectsASearchVariableThatIsNeitherIdPhoneNumberNorEmail()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeeAsync("not-a-valid-search-term", TestContext.Current.CancellationToken);

        Assert.Equal(404, result.Status);
        dbUtils.Verify(d => d.GetEmployeeAsync(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    [InlineData("{3FA85F64-5717-4562-B3FC-2C963F66AFA6}")]
    [InlineData("3fa85f6457174562b3fc2c963f66afa6")]
    public async Task GetEmployeeAsync_LooksUpByEmployeeId_ForAnyGuidSpelling(string searchTerm)
    {
        var captured = await CaptureLookup(searchTerm);

        Assert.Equal(new EmployeeLookup(EmployeeId: EmployeeId), captured);
    }

    [Fact]
    public async Task GetEmployeeAsync_LooksUpByPhoneNumber_ForADigitsOnlySearchTerm()
    {
        Assert.Equal(new EmployeeLookup(PhoneNumber: "123456789"), await CaptureLookup("123456789"));
    }

    [Fact]
    public async Task GetEmployeeAsync_LooksUpByEmail_ForAnEmailShapedSearchVariable()
    {
        Assert.Equal(new EmployeeLookup(Email: "dan@example.com"), await CaptureLookup(" dan@example.com "));
    }

    private static async Task<EmployeeLookup?> CaptureLookup(string searchTerm)
    {
        var dbUtils = new Mock<IDbUtils>();
        EmployeeLookup? captured = null;
        dbUtils.Setup(d => d.GetEmployeeAsync(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()))
            .Callback<EmployeeLookup, CancellationToken>((l, _) => captured = l)
            .ReturnsAsync(new ResponseModel<EmployeeModel>(200, "Employee found.", MakeEmployee()));
        var getting = new EmployeeGetting(dbUtils.Object);

        await getting.GetEmployeeAsync(searchTerm, TestContext.Current.CancellationToken);

        return captured;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetEmployeesAsync_RejectsAnInvalidPageNumber_WithoutTouchingTheDb(int pageNumber)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { PageNumber = pageNumber, PageSize = 10 };

        var result = await getting.GetEmployeesAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetEmployeesAsync_RejectsAnInvalidPageSize_WithoutTouchingTheDb(int pageSize)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { PageNumber = 1, PageSize = pageSize };

        var result = await getting.GetEmployeesAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesAsync_RejectsAnUndefinedSortColumn_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SortColumn = (EmployeeSortColumn)7 };

        var result = await getting.GetEmployeesAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesAsync_RejectsAnUndefinedSortDirection_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SortDirection = (SortDirection)7 };

        var result = await getting.GetEmployeesAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEmployeesAsync_TreatsABlankSearchTermAsNoSearch(string? searchTerm)
    {
        var dbUtils = new Mock<IDbUtils>();
        GetEmployeesRequest? captured = null;
        dbUtils.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!"));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SearchTerm = searchTerm };

        await getting.GetEmployeesAsync(request, TestContext.Current.CancellationToken);

        Assert.Null(captured!.SearchTerm);
    }

    [Fact]
    public async Task GetEmployeesAsync_RejectsASearchTermLongerThanTheProcParameter_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new GetEmployeesRequest { SearchTerm = new string('a', 255) };

        var result = await getting.GetEmployeesAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesAsync_ReturnsWhateverTheDbLayerReturns()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
            new PagedResponse<EmployeeModel>([], 0, 1, 10));
        var request = new GetEmployeesRequest { PageNumber = 1, PageSize = 10 };
        dbUtils.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesAsync(request, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_RejectsAnUndefinedSortColumn_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new ExportEmployeesRequest { SortColumn = (EmployeeSortColumn)7 };

        var result = await getting.GetEmployeesForExportAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_IgnoresPagingAndRequestsTheFullCappedResultInOneCall()
    {
        var dbUtils = new Mock<IDbUtils>();
        GetEmployeesRequest? captured = null;
        dbUtils.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([], 0, 1, 5000)));
        var getting = new EmployeeGetting(dbUtils.Object);
        var request = new ExportEmployeesRequest
        {
            SearchTerm = " dan ", SortColumn = EmployeeSortColumn.Email, SortDirection = SortDirection.Desc
        };

        await getting.GetEmployeesForExportAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(1, captured!.PageNumber);
        Assert.Equal(5000, captured.PageSize);
        Assert.Equal("dan", captured.SearchTerm);
        Assert.Equal(EmployeeSortColumn.Email, captured.SortColumn);
        Assert.Equal(SortDirection.Desc, captured.SortDirection);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_ReturnsCsvBuiltFromTheDbLayersPagedItems()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "Success!",
                new PagedResponse<EmployeeModel>([MakeEmployee()], 1, 1, 5000)));
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesForExportAsync(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        Assert.Contains("Dan", result.Data);
        Assert.Contains("Frunza", result.Data);
    }

    [Fact]
    public async Task GetEmployeesForExportAsync_PassesThroughADbLayerFailureUnchanged()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(500, "Something went wrong."));
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeesForExportAsync(new ExportEmployeesRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(500, result.Status);
        Assert.Equal("Something went wrong.", result.ResponseMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAllEmployeeAuditLogAsync_RejectsAnInvalidPageNumber_WithoutTouchingTheDb(int pageNumber)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetAllEmployeeAuditLogAsync(pageNumber, 10, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetAllEmployeeAuditLogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetAllEmployeeAuditLogAsync_RejectsAnInvalidPageSize_WithoutTouchingTheDb(int pageSize)
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetAllEmployeeAuditLogAsync(1, pageSize, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetAllEmployeeAuditLogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeeAuditLogAsync_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetEmployeeAuditLogAsync(Guid.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.GetEmployeeAuditLogAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllEmployeeAuditLogAsync_ReturnsWhateverTheDbLayerReturns()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(200, "Success!",
            new PagedResponse<GlobalAuditLogEntry>([], 0, 1, 10));
        dbUtils.Setup(d => d.GetAllEmployeeAuditLogAsync(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var getting = new EmployeeGetting(dbUtils.Object);

        var result = await getting.GetAllEmployeeAuditLogAsync(1, 10, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        dbUtils.Verify(d => d.GetAllEmployeeAuditLogAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
