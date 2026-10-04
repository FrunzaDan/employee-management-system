using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.OrgFunctions;

public class OrgFunctionsTests
{
    private static readonly Guid Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private static string TooLong(int maxLength) => new('a', maxLength + 1);
    private static string AtLimit(int maxLength) => new('a', maxLength);

    [Fact]
    public async Task CreateOfficeAsync_ReturnsTheDbGeneratedOfficeId()
    {
        var officeId = Guid.NewGuid();
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Office created successfully.", officeId));
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.CreateOfficeAsync(new CreateOfficeRequest { Name = "HQ" },
            TestContext.Current.CancellationToken);

        Assert.Equal(officeId, result.Data);
    }

    public static TheoryData<CreateOfficeRequest, string> InvalidCreateOfficeRequests => new()
    {
        { new CreateOfficeRequest { Name = null }, "Office name is required." },
        { new CreateOfficeRequest { Name = " " }, "Office name is required." },
        { new CreateOfficeRequest { Name = TooLong(FieldLengthConstants.OfficeName) }, "Office name is too long." },
        { new CreateOfficeRequest { Name = "HQ", City = TooLong(FieldLengthConstants.City) }, "City is too long." },
        { new CreateOfficeRequest { Name = "HQ", Country = TooLong(FieldLengthConstants.Country) }, "Country is too long." }
    };

    [Theory]
    [MemberData(nameof(InvalidCreateOfficeRequests))]
    public async Task CreateOfficeAsync_RejectsAnInvalidRequest_WithoutTouchingTheDb(CreateOfficeRequest request,
        string expectedMessage)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.CreateOfficeAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        dbUtils.Verify(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOfficeAsync_AcceptsEveryFieldAtItsMaximumLength()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Office created successfully.", Id));
        var functions = new OfficeFunctions(dbUtils.Object);
        var request = new CreateOfficeRequest
        {
            Name = AtLimit(FieldLengthConstants.OfficeName),
            City = AtLimit(FieldLengthConstants.City),
            Country = AtLimit(FieldLengthConstants.Country)
        };

        var result = await functions.CreateOfficeAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        dbUtils.Verify(d => d.CreateOfficeAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static TheoryData<UpdateOfficeRequest, string> InvalidUpdateOfficeRequests => new()
    {
        { new UpdateOfficeRequest { Name = "HQ" }, "A valid office ID is required." },
        { new UpdateOfficeRequest { OfficeId = Id, Name = " " }, "Office name cannot be blank." },
        { new UpdateOfficeRequest { OfficeId = Id, Name = TooLong(FieldLengthConstants.OfficeName) }, "Office name is too long." },
        { new UpdateOfficeRequest { OfficeId = Id, City = TooLong(FieldLengthConstants.City) }, "City is too long." },
        { new UpdateOfficeRequest { OfficeId = Id, Country = TooLong(FieldLengthConstants.Country) }, "Country is too long." }
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateOfficeRequests))]
    public async Task UpdateOfficeAsync_RejectsAnInvalidRequest_WithoutTouchingTheDb(UpdateOfficeRequest request,
        string expectedMessage)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.UpdateOfficeAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateOfficeAsync(It.IsAny<UpdateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOfficeAsync_TreatsMissingFieldsAsUnchanged_AndPassesThePatchOn()
    {
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<object>(200, "Office updated successfully.");
        dbUtils.Setup(d => d.UpdateOfficeAsync(It.IsAny<UpdateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.UpdateOfficeAsync(new UpdateOfficeRequest { OfficeId = Id, City = "Iasi" },
            TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
    }

    public static TheoryData<CreateDepartmentRequest, string> InvalidCreateDepartmentRequests => new()
    {
        { new CreateDepartmentRequest { Name = null }, "Department name is required." },
        { new CreateDepartmentRequest { Name = " " }, "Department name is required." },
        { new CreateDepartmentRequest { Name = TooLong(FieldLengthConstants.DepartmentName) }, "Department name is too long." }
    };

    [Theory]
    [MemberData(nameof(InvalidCreateDepartmentRequests))]
    public async Task CreateDepartmentAsync_RejectsAnInvalidRequest_WithoutTouchingTheDb(CreateDepartmentRequest request,
        string expectedMessage)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new DepartmentFunctions(dbUtils.Object);

        var result = await functions.CreateDepartmentAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        dbUtils.Verify(d => d.CreateDepartmentAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateDepartmentAsync_ReturnsTheDbGeneratedDepartmentId()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CreateDepartmentAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Department created successfully.", Id));
        var functions = new DepartmentFunctions(dbUtils.Object);

        var result = await functions.CreateDepartmentAsync(
            new CreateDepartmentRequest { Name = AtLimit(FieldLengthConstants.DepartmentName) },
            TestContext.Current.CancellationToken);

        Assert.Equal(Id, result.Data);
    }

    public static TheoryData<UpdateDepartmentRequest, string> InvalidUpdateDepartmentRequests => new()
    {
        { new UpdateDepartmentRequest { Name = "Sales" }, "A valid department ID is required." },
        { new UpdateDepartmentRequest { DepartmentId = Id, Name = " " }, "Department name cannot be blank." },
        { new UpdateDepartmentRequest { DepartmentId = Id, Name = TooLong(FieldLengthConstants.DepartmentName) }, "Department name is too long." }
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateDepartmentRequests))]
    public async Task UpdateDepartmentAsync_RejectsAnInvalidRequest_WithoutTouchingTheDb(UpdateDepartmentRequest request,
        string expectedMessage)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new DepartmentFunctions(dbUtils.Object);

        var result = await functions.UpdateDepartmentAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateDepartmentAsync(It.IsAny<UpdateDepartmentRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    public static TheoryData<CreateCostCenterRequest, string> InvalidCreateCostCenterRequests => new()
    {
        { new CreateCostCenterRequest { Name = "Sales" }, "Cost center code is required." },
        { new CreateCostCenterRequest { Code = " " }, "Cost center code is required." },
        { new CreateCostCenterRequest { Code = TooLong(FieldLengthConstants.CostCenterCode) }, "Cost center code is too long." },
        { new CreateCostCenterRequest { Code = "CC-1", Name = TooLong(FieldLengthConstants.CostCenterName) }, "Cost center name is too long." }
    };

    [Theory]
    [MemberData(nameof(InvalidCreateCostCenterRequests))]
    public async Task CreateCostCenterAsync_RejectsAnInvalidRequest_WithoutTouchingTheDb(CreateCostCenterRequest request,
        string expectedMessage)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new CostCenterFunctions(dbUtils.Object);

        var result = await functions.CreateCostCenterAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        dbUtils.Verify(d => d.CreateCostCenterAsync(It.IsAny<CreateCostCenterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCostCenterAsync_AllowsACostCenterWithoutAName()
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CreateCostCenterAsync(It.IsAny<CreateCostCenterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Cost center created successfully.", Id));
        var functions = new CostCenterFunctions(dbUtils.Object);

        var result = await functions.CreateCostCenterAsync(new CreateCostCenterRequest { Code = "CC-1" },
            TestContext.Current.CancellationToken);

        Assert.Equal(Id, result.Data);
    }

    public static TheoryData<UpdateCostCenterRequest, string> InvalidUpdateCostCenterRequests => new()
    {
        { new UpdateCostCenterRequest { Code = "CC-1" }, "A valid cost center ID is required." },
        { new UpdateCostCenterRequest { CostCenterId = Id, Code = " " }, "Cost center code cannot be blank." },
        { new UpdateCostCenterRequest { CostCenterId = Id, Code = TooLong(FieldLengthConstants.CostCenterCode) }, "Cost center code is too long." },
        { new UpdateCostCenterRequest { CostCenterId = Id, Name = TooLong(FieldLengthConstants.CostCenterName) }, "Cost center name is too long." }
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateCostCenterRequests))]
    public async Task UpdateCostCenterAsync_RejectsAnInvalidRequest_WithoutTouchingTheDb(UpdateCostCenterRequest request,
        string expectedMessage)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new CostCenterFunctions(dbUtils.Object);

        var result = await functions.UpdateCostCenterAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateCostCenterAsync(It.IsAny<UpdateCostCenterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    public static TheoryData<string> CallsTakingAnId =>
    [
        "GetOffice", "DeleteOffice", "GetEmployeesByOffice",
        "GetDepartment", "DeleteDepartment", "GetEmployeesByDepartment",
        "GetCostCenter", "DeleteCostCenter", "GetEmployeesByCostCenter"
    ];

    private static Task<int> CallWithId(string call, IDbUtils dbUtils, Guid id, CancellationToken cancellationToken)
    {
        var offices = new OfficeFunctions(dbUtils);
        var departments = new DepartmentFunctions(dbUtils);
        var costCenters = new CostCenterFunctions(dbUtils);
        return call switch
        {
            "GetOffice" => Status(offices.GetOfficeAsync(id, cancellationToken)),
            "DeleteOffice" => Status(offices.DeleteOfficeAsync(id, cancellationToken)),
            "GetEmployeesByOffice" => Status(offices.GetEmployeesByOfficeAsync(id, cancellationToken)),
            "GetDepartment" => Status(departments.GetDepartmentAsync(id, cancellationToken)),
            "DeleteDepartment" => Status(departments.DeleteDepartmentAsync(id, cancellationToken)),
            "GetEmployeesByDepartment" => Status(departments.GetEmployeesByDepartmentAsync(id, cancellationToken)),
            "GetCostCenter" => Status(costCenters.GetCostCenterAsync(id, cancellationToken)),
            "DeleteCostCenter" => Status(costCenters.DeleteCostCenterAsync(id, cancellationToken)),
            "GetEmployeesByCostCenter" => Status(costCenters.GetEmployeesByCostCenterAsync(id, cancellationToken)),
            _ => throw new ArgumentOutOfRangeException(nameof(call), call, null)
        };
    }

    private static async Task<int> Status<T>(Task<ResponseModel<T>> response) => (await response).Status;

    [Theory]
    [MemberData(nameof(CallsTakingAnId))]
    public async Task EveryCallTakingAnId_RejectsAnEmptyId_WithoutTouchingTheDb(string call)
    {
        var dbUtils = new Mock<IDbUtils>(MockBehavior.Strict);

        var status = await CallWithId(call, dbUtils.Object, Guid.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(400, status);
    }

    [Theory]
    [MemberData(nameof(CallsTakingAnId))]
    public async Task EveryCallTakingAnId_PassesTheDbResponseOn_ForAValidId(string call)
    {
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.GetOfficeAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<OfficeModel>(404, "Office not found."));
        dbUtils.Setup(d => d.DeleteOfficeAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(404, "Office not found."));
        dbUtils.Setup(d => d.GetEmployeesByOfficeAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(404, "Office not found."));
        dbUtils.Setup(d => d.GetDepartmentAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<DepartmentModel>(404, "Department not found."));
        dbUtils.Setup(d => d.DeleteDepartmentAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(404, "Department not found."));
        dbUtils.Setup(d => d.GetEmployeesByDepartmentAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(404, "Department not found."));
        dbUtils.Setup(d => d.GetCostCenterAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<CostCenterModel>(404, "Cost center not found."));
        dbUtils.Setup(d => d.DeleteCostCenterAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(404, "Cost center not found."));
        dbUtils.Setup(d => d.GetEmployeesByCostCenterAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(404, "Cost center not found."));

        var status = await CallWithId(call, dbUtils.Object, Id, TestContext.Current.CancellationToken);

        Assert.Equal(404, status);
    }

    [Fact]
    public async Task GetAllCalls_DelegateToTheDbLayer()
    {
        var dbUtils = new Mock<IDbUtils>();
        var offices = new ResponseModel<IReadOnlyList<OfficeModel>>(200, "0 offices found.", []);
        var departments = new ResponseModel<IReadOnlyList<DepartmentModel>>(200, "0 departments found.", []);
        var costCenters = new ResponseModel<IReadOnlyList<CostCenterModel>>(200, "0 cost centers found.", []);
        dbUtils.Setup(d => d.GetOfficesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);
        dbUtils.Setup(d => d.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(departments);
        dbUtils.Setup(d => d.GetCostCentersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(costCenters);
        var cancellationToken = TestContext.Current.CancellationToken;

        Assert.Same(offices, await new OfficeFunctions(dbUtils.Object).GetOfficesAsync(cancellationToken));
        Assert.Same(departments, await new DepartmentFunctions(dbUtils.Object).GetDepartmentsAsync(cancellationToken));
        Assert.Same(costCenters, await new CostCenterFunctions(dbUtils.Object).GetCostCentersAsync(cancellationToken));
    }
}
