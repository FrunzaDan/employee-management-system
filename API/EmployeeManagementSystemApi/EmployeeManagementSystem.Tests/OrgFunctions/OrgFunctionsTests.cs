using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.OrgFunctions;

// Office/Department/CostCenter functions share one shape (see OrgFunctions), so one
// representative per rule is enough: required name/code on create, lengths, and the ID guard.
public class OrgFunctionsTests
{
    [Fact]
    public async Task CreateOfficeFunction_ReturnsTheDbGeneratedOfficeId()
    {
        var officeId = Guid.NewGuid();
        var dbUtils = new Mock<IDbUtils>();
        dbUtils.Setup(d => d.CreateOffice(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Office created successfully.", officeId));
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.CreateOfficeFunction(new CreateOfficeRequest { Name = "HQ" },
            TestContext.Current.CancellationToken);

        Assert.Equal(officeId, result.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task CreateOfficeFunction_RejectsAMissingName_WithoutTouchingTheDb(string? name)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.CreateOfficeFunction(new CreateOfficeRequest { Name = name },
            TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Office name is required.", result.ResponseMessage);
        dbUtils.Verify(d => d.CreateOffice(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCostCenterFunction_RejectsAMissingCode_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new CostCenterFunctions(dbUtils.Object);

        var result = await functions.CreateCostCenterFunction(new CreateCostCenterRequest { Name = "Sales" },
            TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Cost center code is required.", result.ResponseMessage);
        dbUtils.Verify(d => d.CreateCostCenter(It.IsAny<CreateCostCenterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EditDepartmentFunction_RejectsAnOverLengthName_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new DepartmentFunctions(dbUtils.Object);
        var request = new UpdateDepartmentRequest
        {
            DepartmentId = Guid.NewGuid(),
            Name = new string('a', FieldLengthConstants.DepartmentName + 1)
        };

        var result = await functions.EditDepartmentFunction(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Department name is too long.", result.ResponseMessage);
        dbUtils.Verify(d => d.EditDepartment(It.IsAny<UpdateDepartmentRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EditOfficeFunction_RejectsAnEmptyOfficeId_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.EditOfficeFunction(new UpdateOfficeRequest { Name = "HQ" },
            TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("A valid office ID is required.", result.ResponseMessage);
        dbUtils.Verify(d => d.EditOffice(It.IsAny<UpdateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCostCenterFunction_RejectsAnEmptyCostCenterId_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new CostCenterFunctions(dbUtils.Object);

        var result = await functions.DeleteCostCenterFunction(Guid.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.DeleteCostCenter(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesByDepartmentFunction_DelegatesToTheDbLayer()
    {
        var departmentId = Guid.NewGuid();
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(200, "0 employees found.", []);
        dbUtils.Setup(d => d.GetEmployeesByDepartment(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var functions = new DepartmentFunctions(dbUtils.Object);

        var result = await functions.GetEmployeesByDepartmentFunction(departmentId,
            TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
    }
}
