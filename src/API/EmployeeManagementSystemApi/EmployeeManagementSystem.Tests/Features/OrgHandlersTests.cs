using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.CostCenters;
using EmployeeManagementSystem.BusinessLogic.Features.Departments;
using EmployeeManagementSystem.BusinessLogic.Features.Offices;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features;

// Offices, departments and cost centers follow the same rules, so their handlers are tested side by side.
public class OrgHandlersTests
{
    private static readonly Guid Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private static string TooLong(int maxLength) => new('a', maxLength + 1);
    private static string AtLimit(int maxLength) => new('a', maxLength);

    [Fact]
    public async Task CreateOfficeAsync_ReturnsTheDbGeneratedOfficeId()
    {
        var officeId = Guid.NewGuid();
        var offices = new Mock<IOfficeRepository>();
        offices.Setup(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Office created successfully.", officeId));
        var handler = new CreateOfficeHandler(offices.Object);

        var result = await handler.HandleAsync(new CreateOfficeRequest { Name = "HQ" },
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
        var offices = new Mock<IOfficeRepository>();
        var handler = new CreateOfficeHandler(offices.Object);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        offices.Verify(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOfficeAsync_AcceptsEveryFieldAtItsMaximumLength()
    {
        var offices = new Mock<IOfficeRepository>();
        offices.Setup(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Office created successfully.", Id));
        var handler = new CreateOfficeHandler(offices.Object);
        var request = new CreateOfficeRequest
        {
            Name = AtLimit(FieldLengthConstants.OfficeName),
            City = AtLimit(FieldLengthConstants.City),
            Country = AtLimit(FieldLengthConstants.Country)
        };

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Status);
        offices.Verify(d => d.CreateOfficeAsync(request, It.IsAny<CancellationToken>()), Times.Once);
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
        var offices = new Mock<IOfficeRepository>();
        var handler = new UpdateOfficeHandler(offices.Object);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        offices.Verify(d => d.UpdateOfficeAsync(It.IsAny<UpdateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOfficeAsync_TreatsMissingFieldsAsUnchanged_AndPassesThePatchOn()
    {
        var offices = new Mock<IOfficeRepository>();
        var expected = new ResponseModel<object>(200, "Office updated successfully.");
        offices.Setup(d => d.UpdateOfficeAsync(It.IsAny<UpdateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new UpdateOfficeHandler(offices.Object);

        var result = await handler.HandleAsync(new UpdateOfficeRequest { OfficeId = Id, City = "Iasi" },
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
        var departments = new Mock<IDepartmentRepository>();
        var handler = new CreateDepartmentHandler(departments.Object);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        departments.Verify(d => d.CreateDepartmentAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateDepartmentAsync_ReturnsTheDbGeneratedDepartmentId()
    {
        var departments = new Mock<IDepartmentRepository>();
        departments.Setup(d => d.CreateDepartmentAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Department created successfully.", Id));
        var handler = new CreateDepartmentHandler(departments.Object);

        var result = await handler.HandleAsync(
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
        var departments = new Mock<IDepartmentRepository>();
        var handler = new UpdateDepartmentHandler(departments.Object);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        departments.Verify(d => d.UpdateDepartmentAsync(It.IsAny<UpdateDepartmentRequest>(), It.IsAny<CancellationToken>()),
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
        var costCenters = new Mock<ICostCenterRepository>();
        var handler = new CreateCostCenterHandler(costCenters.Object);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        costCenters.Verify(d => d.CreateCostCenterAsync(It.IsAny<CreateCostCenterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCostCenterAsync_AllowsACostCenterWithoutAName()
    {
        var costCenters = new Mock<ICostCenterRepository>();
        costCenters.Setup(d => d.CreateCostCenterAsync(It.IsAny<CreateCostCenterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Cost center created successfully.", Id));
        var handler = new CreateCostCenterHandler(costCenters.Object);

        var result = await handler.HandleAsync(new CreateCostCenterRequest { Code = "CC-1" },
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
        var costCenters = new Mock<ICostCenterRepository>();
        var handler = new UpdateCostCenterHandler(costCenters.Object);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal(expectedMessage, result.ResponseMessage);
        costCenters.Verify(d => d.UpdateCostCenterAsync(It.IsAny<UpdateCostCenterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    public static TheoryData<string> CallsTakingAnId =>
    [
        "GetOffice", "DeleteOffice", "GetEmployeesByOffice",
        "GetDepartment", "DeleteDepartment", "GetEmployeesByDepartment",
        "GetCostCenter", "DeleteCostCenter", "GetEmployeesByCostCenter"
    ];

    private static Task<int> CallWithId(string call, IOfficeRepository offices, IDepartmentRepository departments,
        ICostCenterRepository costCenters, Guid id, CancellationToken cancellationToken) =>
        call switch
        {
            "GetOffice" => Status(new GetOfficeHandler(offices).HandleAsync(id, cancellationToken)),
            "DeleteOffice" => Status(new DeleteOfficeHandler(offices).HandleAsync(id, cancellationToken)),
            "GetEmployeesByOffice" => Status(new GetEmployeesByOfficeHandler(offices).HandleAsync(id, cancellationToken)),
            "GetDepartment" => Status(new GetDepartmentHandler(departments).HandleAsync(id, cancellationToken)),
            "DeleteDepartment" => Status(new DeleteDepartmentHandler(departments).HandleAsync(id, cancellationToken)),
            "GetEmployeesByDepartment" =>
                Status(new GetEmployeesByDepartmentHandler(departments).HandleAsync(id, cancellationToken)),
            "GetCostCenter" => Status(new GetCostCenterHandler(costCenters).HandleAsync(id, cancellationToken)),
            "DeleteCostCenter" => Status(new DeleteCostCenterHandler(costCenters).HandleAsync(id, cancellationToken)),
            "GetEmployeesByCostCenter" =>
                Status(new GetEmployeesByCostCenterHandler(costCenters).HandleAsync(id, cancellationToken)),
            _ => throw new ArgumentOutOfRangeException(nameof(call), call, null)
        };

    private static async Task<int> Status<T>(Task<ResponseModel<T>> response) => (await response).Status;

    [Theory]
    [MemberData(nameof(CallsTakingAnId))]
    public async Task EveryCallTakingAnId_RejectsAnEmptyId_WithoutTouchingTheDb(string call)
    {
        var offices = new Mock<IOfficeRepository>(MockBehavior.Strict);
        var departments = new Mock<IDepartmentRepository>(MockBehavior.Strict);
        var costCenters = new Mock<ICostCenterRepository>(MockBehavior.Strict);

        var status = await CallWithId(call, offices.Object, departments.Object, costCenters.Object, Guid.Empty,
            TestContext.Current.CancellationToken);

        Assert.Equal(400, status);
    }

    [Theory]
    [MemberData(nameof(CallsTakingAnId))]
    public async Task EveryCallTakingAnId_PassesTheDbResponseOn_ForAValidId(string call)
    {
        var offices = new Mock<IOfficeRepository>();
        var departments = new Mock<IDepartmentRepository>();
        var costCenters = new Mock<ICostCenterRepository>();
        offices.Setup(d => d.GetOfficeAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<OfficeModel>(404, "Office not found."));
        offices.Setup(d => d.DeleteOfficeAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(404, "Office not found."));
        offices.Setup(d => d.GetEmployeesByOfficeAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(404, "Office not found."));
        departments.Setup(d => d.GetDepartmentAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<DepartmentModel>(404, "Department not found."));
        departments.Setup(d => d.DeleteDepartmentAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(404, "Department not found."));
        departments.Setup(d => d.GetEmployeesByDepartmentAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(404, "Department not found."));
        costCenters.Setup(d => d.GetCostCenterAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<CostCenterModel>(404, "Cost center not found."));
        costCenters.Setup(d => d.DeleteCostCenterAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(404, "Cost center not found."));
        costCenters.Setup(d => d.GetEmployeesByCostCenterAsync(Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(404, "Cost center not found."));

        var status = await CallWithId(call, offices.Object, departments.Object, costCenters.Object, Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(404, status);
    }

    [Fact]
    public async Task GetAllHandlers_DelegateToTheirRepository()
    {
        var officeRepository = new Mock<IOfficeRepository>();
        var departmentRepository = new Mock<IDepartmentRepository>();
        var costCenterRepository = new Mock<ICostCenterRepository>();
        var offices = new ResponseModel<IReadOnlyList<OfficeModel>>(200, "0 offices found.", []);
        var departments = new ResponseModel<IReadOnlyList<DepartmentModel>>(200, "0 departments found.", []);
        var costCenters = new ResponseModel<IReadOnlyList<CostCenterModel>>(200, "0 cost centers found.", []);
        officeRepository.Setup(d => d.GetOfficesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);
        departmentRepository.Setup(d => d.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(departments);
        costCenterRepository.Setup(d => d.GetCostCentersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(costCenters);
        var cancellationToken = TestContext.Current.CancellationToken;

        Assert.Same(offices, await new GetOfficesHandler(officeRepository.Object).HandleAsync(cancellationToken));
        Assert.Same(departments, await new GetDepartmentsHandler(departmentRepository.Object).HandleAsync(cancellationToken));
        Assert.Same(costCenters, await new GetCostCentersHandler(costCenterRepository.Object).HandleAsync(cancellationToken));
    }
}
