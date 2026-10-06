using System.Net;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Endpoints;

// Offices, departments and cost centers: the same six endpoints each, as the admin pages call them.
public class OrgEndpointTests
{
    private static readonly Guid OrgId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static ResponseModel<object> Ok() => new(200, "Done.");

    private static IReadOnlyList<EmployeeSummaryModel> OneEmployee() =>
    [
        new EmployeeSummaryModel
        {
            EmployeeId = Guid.NewGuid(), FirstName = "Ana", LastName = "Pop", Email = "ana@example.com",
            Status = EmployeeStatus.Active
        }
    ];

    // ---- Offices ----

    [Fact]
    public async Task Office_GetAll_ReturnsTheOfficesWithTheirTotals()
    {
        await using var api = new ApiHost();
        api.Offices.Setup(d => d.GetOfficesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<OfficeModel>>(200, "", [
                new OfficeModel { OfficeId = OrgId, Name = "Cluj HQ", City = "Cluj-Napoca", EmployeeCount = 3, TotalGrossSalary = 12000m }
            ]));

        var office = (await ApiHost.ReadEnvelopeAsync(await api.GetAsync("/api/office/all"))).GetProperty("data")[0];

        Assert.Equal(OrgId, office.GetProperty("officeId").GetGuid());
        Assert.Equal("Cluj HQ", office.GetProperty("name").GetString());
        Assert.Equal(3, office.GetProperty("employeeCount").GetInt32());
        Assert.Equal(12000m, office.GetProperty("totalGrossSalary").GetDecimal());
    }

    [Fact]
    public async Task Office_GetGet_ReturnsThatOffice()
    {
        await using var api = new ApiHost();
        api.Offices.Setup(d => d.GetOfficeAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<OfficeModel>(200, "", new OfficeModel { OfficeId = OrgId, Name = "Cluj HQ" }));

        var response = await api.GetAsync($"/api/office/get?officeId={OrgId}");

        Assert.Equal("Cluj HQ", (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public async Task Office_PostCreate_SendsTheBody_AndReturnsTheNewId()
    {
        await using var api = new ApiHost();
        CreateOfficeRequest? sent = null;
        api.Offices.Setup(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateOfficeRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(new ResponseModel<Guid?>(200, "", OrgId));

        var response = await api.PostAsync("/api/office/create", new { name = "Cluj HQ", city = "Cluj-Napoca", country = "Romania" });

        Assert.Equal(OrgId, (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetGuid());
        Assert.NotNull(sent);
        Assert.Equal("Cluj HQ", sent.Name);
        Assert.Equal("Cluj-Napoca", sent.City);
        Assert.Equal("Romania", sent.Country);
    }

    [Fact]
    public async Task Office_PostCreate_WithoutAName_IsA400Problem_WithoutTouchingTheDb()
    {
        await using var api = new ApiHost();

        var response = await api.PostAsync("/api/office/create", new { name = " " });

        var problem = await ApiHost.ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal("Office name is required.", problem.GetProperty("detail").GetString());
        api.Offices.Verify(d => d.CreateOfficeAsync(It.IsAny<CreateOfficeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Office_PatchUpdate_SendsTheBody()
    {
        await using var api = new ApiHost();
        UpdateOfficeRequest? sent = null;
        api.Offices.Setup(d => d.UpdateOfficeAsync(It.IsAny<UpdateOfficeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<UpdateOfficeRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(Ok());

        await ApiHost.ReadEnvelopeAsync(await api.PatchAsync("/api/office/update", new { officeId = OrgId, name = "Iasi" }));

        Assert.NotNull(sent);
        Assert.Equal(OrgId, sent.OfficeId);
        Assert.Equal("Iasi", sent.Name);
    }

    [Fact]
    public async Task Office_DeleteDelete_DeletesThatOffice()
    {
        await using var api = new ApiHost();
        api.Offices.Setup(d => d.DeleteOfficeAsync(OrgId, It.IsAny<CancellationToken>())).ReturnsAsync(Ok());

        await ApiHost.ReadEnvelopeAsync(await api.DeleteAsync($"/api/office/delete?officeId={OrgId}"));

        api.Offices.Verify(d => d.DeleteOfficeAsync(OrgId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Office_DeleteDelete_AnOfficeInUse_IsA409Problem()
    {
        await using var api = new ApiHost();
        api.Offices.Setup(d => d.DeleteOfficeAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(409, "The office still has employees."));

        var response = await api.DeleteAsync($"/api/office/delete?officeId={OrgId}");

        var problem = await ApiHost.ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.Equal("The office still has employees.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Office_GetEmployees_ReturnsWhoWorksThere()
    {
        await using var api = new ApiHost();
        api.Offices.Setup(d => d.GetEmployeesByOfficeAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(200, "", OneEmployee()));

        var response = await api.GetAsync($"/api/office/employees?officeId={OrgId}");

        var employee = (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data")[0];
        Assert.Equal("ana@example.com", employee.GetProperty("email").GetString());
        Assert.Equal(1901, employee.GetProperty("status").GetInt32());
    }

    // ---- Departments ----

    [Fact]
    public async Task Department_GetAll_ReturnsTheDepartments()
    {
        await using var api = new ApiHost();
        api.Departments.Setup(d => d.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<DepartmentModel>>(200, "", [
                new DepartmentModel { DepartmentId = OrgId, Name = "IT", EmployeeCount = 2 }
            ]));

        var department = (await ApiHost.ReadEnvelopeAsync(await api.GetAsync("/api/department/all"))).GetProperty("data")[0];

        Assert.Equal(OrgId, department.GetProperty("departmentId").GetGuid());
        Assert.Equal("IT", department.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Department_GetGet_ReturnsThatDepartment()
    {
        await using var api = new ApiHost();
        api.Departments.Setup(d => d.GetDepartmentAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<DepartmentModel>(200, "", new DepartmentModel { DepartmentId = OrgId, Name = "IT" }));

        var response = await api.GetAsync($"/api/department/get?departmentId={OrgId}");

        Assert.Equal("IT", (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public async Task Department_PostCreate_SendsTheName_AndReturnsTheNewId()
    {
        await using var api = new ApiHost();
        api.Departments.Setup(d => d.CreateDepartmentAsync(It.Is<CreateDepartmentRequest>(r => r.Name == "IT"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "", OrgId));

        var response = await api.PostAsync("/api/department/create", new { name = "IT" });

        Assert.Equal(OrgId, (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetGuid());
    }

    [Fact]
    public async Task Department_PatchUpdate_SendsTheBody()
    {
        await using var api = new ApiHost();
        api.Departments.Setup(d => d.UpdateDepartmentAsync(
                It.Is<UpdateDepartmentRequest>(r => r.DepartmentId == OrgId && r.Name == "HR"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Ok());

        var response = await api.PatchAsync("/api/department/update", new { departmentId = OrgId, name = "HR" });

        await ApiHost.ReadEnvelopeAsync(response);
        api.Departments.Verify(d => d.UpdateDepartmentAsync(It.IsAny<UpdateDepartmentRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Department_DeleteDelete_DeletesThatDepartment()
    {
        await using var api = new ApiHost();
        api.Departments.Setup(d => d.DeleteDepartmentAsync(OrgId, It.IsAny<CancellationToken>())).ReturnsAsync(Ok());

        await ApiHost.ReadEnvelopeAsync(await api.DeleteAsync($"/api/department/delete?departmentId={OrgId}"));

        api.Departments.Verify(d => d.DeleteDepartmentAsync(OrgId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Department_GetEmployees_ReturnsWhoWorksThere()
    {
        await using var api = new ApiHost();
        api.Departments.Setup(d => d.GetEmployeesByDepartmentAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(200, "", OneEmployee()));

        var response = await api.GetAsync($"/api/department/employees?departmentId={OrgId}");

        Assert.Equal(1, (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetArrayLength());
    }

    // ---- Cost centers ----

    [Fact]
    public async Task CostCenter_GetAll_ReturnsTheCostCenters()
    {
        await using var api = new ApiHost();
        api.CostCenters.Setup(d => d.GetCostCentersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<CostCenterModel>>(200, "", [
                new CostCenterModel { CostCenterId = OrgId, Code = "CC-01", Name = "Engineering" }
            ]));

        var costCenter = (await ApiHost.ReadEnvelopeAsync(await api.GetAsync("/api/cost-center/all"))).GetProperty("data")[0];

        Assert.Equal(OrgId, costCenter.GetProperty("costCenterId").GetGuid());
        Assert.Equal("CC-01", costCenter.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CostCenter_GetGet_ReturnsThatCostCenter()
    {
        await using var api = new ApiHost();
        api.CostCenters.Setup(d => d.GetCostCenterAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<CostCenterModel>(200, "", new CostCenterModel { CostCenterId = OrgId, Code = "CC-01" }));

        var response = await api.GetAsync($"/api/cost-center/get?costCenterId={OrgId}");

        Assert.Equal("CC-01", (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetProperty("code").GetString());
    }

    [Fact]
    public async Task CostCenter_PostCreate_SendsTheBody_AndReturnsTheNewId()
    {
        await using var api = new ApiHost();
        api.CostCenters.Setup(d => d.CreateCostCenterAsync(
                It.Is<CreateCostCenterRequest>(r => r.Code == "CC-01" && r.Name == "Engineering"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<Guid?>(200, "", OrgId));

        var response = await api.PostAsync("/api/cost-center/create", new { code = "CC-01", name = "Engineering" });

        Assert.Equal(OrgId, (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetGuid());
    }

    [Fact]
    public async Task CostCenter_PatchUpdate_SendsTheBody()
    {
        await using var api = new ApiHost();
        api.CostCenters.Setup(d => d.UpdateCostCenterAsync(
                It.Is<UpdateCostCenterRequest>(r => r.CostCenterId == OrgId && r.Code == "CC-02"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Ok());

        var response = await api.PatchAsync("/api/cost-center/update", new { costCenterId = OrgId, code = "CC-02" });

        await ApiHost.ReadEnvelopeAsync(response);
        api.CostCenters.Verify(d => d.UpdateCostCenterAsync(It.IsAny<UpdateCostCenterRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CostCenter_DeleteDelete_DeletesThatCostCenter()
    {
        await using var api = new ApiHost();
        api.CostCenters.Setup(d => d.DeleteCostCenterAsync(OrgId, It.IsAny<CancellationToken>())).ReturnsAsync(Ok());

        await ApiHost.ReadEnvelopeAsync(await api.DeleteAsync($"/api/cost-center/delete?costCenterId={OrgId}"));

        api.CostCenters.Verify(d => d.DeleteCostCenterAsync(OrgId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CostCenter_GetEmployees_ReturnsWhoIsBookedThere()
    {
        await using var api = new ApiHost();
        api.CostCenters.Setup(d => d.GetEmployeesByCostCenterAsync(OrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(200, "", OneEmployee()));

        var response = await api.GetAsync($"/api/cost-center/employees?costCenterId={OrgId}");

        Assert.Equal(1, (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetArrayLength());
    }
}
