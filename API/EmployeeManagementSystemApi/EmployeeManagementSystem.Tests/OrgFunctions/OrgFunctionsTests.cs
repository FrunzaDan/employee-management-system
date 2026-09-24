using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.OrgFunctions;

public class OrgFunctionsTests
{
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

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task CreateOfficeAsync_RejectsAMissingName_WithoutTouchingTheDb(string? name)
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.CreateOfficeAsync(new CreateOfficeRequest { Name = name },
            TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Office name is required.", result.ResponseMessage);
        dbUtils.Verify(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCostCenterAsync_RejectsAMissingCode_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new CostCenterFunctions(dbUtils.Object);

        var result = await functions.CreateCostCenterAsync(new CreateCostCenterRequest { Name = "Sales" },
            TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Cost center code is required.", result.ResponseMessage);
        dbUtils.Verify(d => d.CreateCostCenterAsync(It.IsAny<CreateCostCenterRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_RejectsAnOverLengthName_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new DepartmentFunctions(dbUtils.Object);
        var request = new UpdateDepartmentRequest
        {
            DepartmentId = Guid.NewGuid(),
            Name = new string('a', FieldLengthConstants.DepartmentName + 1)
        };

        var result = await functions.UpdateDepartmentAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("Department name is too long.", result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateDepartmentAsync(It.IsAny<UpdateDepartmentRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateOfficeAsync_RejectsAnEmptyOfficeId_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new OfficeFunctions(dbUtils.Object);

        var result = await functions.UpdateOfficeAsync(new UpdateOfficeRequest { Name = "HQ" },
            TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Equal("A valid office ID is required.", result.ResponseMessage);
        dbUtils.Verify(d => d.UpdateOfficeAsync(It.IsAny<UpdateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCostCenterAsync_RejectsAnEmptyCostCenterId_WithoutTouchingTheDb()
    {
        var dbUtils = new Mock<IDbUtils>();
        var functions = new CostCenterFunctions(dbUtils.Object);

        var result = await functions.DeleteCostCenterAsync(Guid.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        dbUtils.Verify(d => d.DeleteCostCenterAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeesByDepartmentAsync_DelegatesToTheDbLayer()
    {
        var departmentId = Guid.NewGuid();
        var dbUtils = new Mock<IDbUtils>();
        var expected = new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(200, "0 employees found.", []);
        dbUtils.Setup(d => d.GetEmployeesByDepartmentAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var functions = new DepartmentFunctions(dbUtils.Object);

        var result = await functions.GetEmployeesByDepartmentAsync(departmentId,
            TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
    }
}
