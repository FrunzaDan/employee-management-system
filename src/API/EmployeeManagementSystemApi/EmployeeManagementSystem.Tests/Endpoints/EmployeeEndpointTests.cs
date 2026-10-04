using System.Net;
using System.Text.Json;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Endpoints;

// Each endpoint as the UI calls it: URL, HTTP method, parameters, the call that reaches the
// data layer, the response shape and the audit entry.
public class EmployeeEndpointTests
{
    private static readonly Guid EmployeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OfficeId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static ResponseModel<object> Ok() => new(200, "Done.");

    private static EmployeeModel SampleEmployee() => new()
    {
        EmployeeId = EmployeeId,
        FirstName = "Ana",
        LastName = "Pop",
        PhoneNumber = "0712345678",
        Email = "ana@example.com",
        Status = EmployeeStatus.Active,
        AccountCreatedAt = new DateTime(2024, 1, 2, 8, 0, 0, DateTimeKind.Utc),
        LastInteractionAt = new DateTime(2024, 3, 4, 8, 0, 0, DateTimeKind.Utc),
        Gender = Gender.Female,
        BirthDate = new DateOnly(1990, 5, 6),
        HireDate = new DateOnly(2024, 1, 2),
        OfficeId = OfficeId,
        OfficeName = "Cluj HQ",
        CurrentGrossSalary = 12000m,
        Address = new AddressModel
        {
            Country = "Romania",
            County = "Cluj",
            City = "Cluj-Napoca",
            PostalCode = "400001",
            Street = "Main",
            StreetNumber = "1"
        },
    };

    [Fact]
    public async Task PostCreate_SendsTheBodyToTheDb_AndReturnsTheNewId()
    {
        await using var api = new ApiHost();
        CreateEmployeeRequest? sent = null;
        api.Db.Setup(d => d.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateEmployeeRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(new ResponseModel<Guid?>(200, "Employee created successfully.", EmployeeId));

        var response = await api.PostAsync("/api/employee/create", new
        {
            firstName = "Ana",
            lastName = "Pop",
            email = "ana@example.com",
            phoneNumber = "0712345678",
            gender = 2,
            hireDate = "2024-01-02",
            officeId = OfficeId,
            address = new
            {
                country = "Romania",
                county = "Cluj",
                city = "Cluj-Napoca",
                postalCode = "400001",
                street = "Main",
                streetNumber = "1"
            },
        });

        var envelope = await ApiHost.ReadEnvelopeAsync(response);
        Assert.Equal(EmployeeId, envelope.GetProperty("data").GetGuid());
        Assert.NotNull(sent);
        Assert.Equal("Ana", sent.FirstName);
        Assert.Equal(Gender.Female, sent.Gender);
        Assert.Equal(new DateOnly(2024, 1, 2), sent.HireDate);
        Assert.Equal(OfficeId, sent.OfficeId);
        Assert.Equal("Cluj-Napoca", sent.Address!.City);
        api.VerifyAudit(EmployeeId, AuditAction.Created, "Email: ana@example.com, Phone number: 0712345678");
    }

    [Fact]
    public async Task GetGet_LooksTheEmployeeUpById_AndReturnsItInTheWireFormat()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.GetEmployeeAsync(new EmployeeLookup(EmployeeId, null, null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<EmployeeModel>(200, "Employee found.", SampleEmployee()));

        var response = await api.GetAsync($"/api/employee/get?searchTerm={EmployeeId}");

        var employee = (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data");
        Assert.Equal(EmployeeId, employee.GetProperty("employeeId").GetGuid());
        Assert.Equal("Ana", employee.GetProperty("firstName").GetString());
        Assert.Equal(1901, employee.GetProperty("status").GetInt32());
        Assert.Equal(2, employee.GetProperty("gender").GetInt32());
        Assert.Equal("2024-01-02", employee.GetProperty("hireDate").GetString());
        Assert.Equal("Cluj HQ", employee.GetProperty("officeName").GetString());
        Assert.Equal(12000m, employee.GetProperty("currentGrossSalary").GetDecimal());
        Assert.Equal(JsonValueKind.Null, employee.GetProperty("departmentId").ValueKind);
        Assert.Equal("Cluj-Napoca", employee.GetProperty("address").GetProperty("city").GetString());
    }

    [Fact]
    public async Task GetGet_AnUnknownEmployee_IsA404Problem()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.GetEmployeeAsync(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<EmployeeModel>(404, "Employee not found."));

        var response = await api.GetAsync("/api/employee/get?searchTerm=ana@example.com");

        var problem = await ApiHost.ReadProblemAsync(response, HttpStatusCode.NotFound);
        Assert.Equal("Employee not found.", problem.GetProperty("detail").GetString());
        api.Db.Verify(d => d.GetEmployeeAsync(new EmployeeLookup(null, null, "ana@example.com"),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetAll_BindsTheUisQueryParameters_AndReturnsThePage()
    {
        await using var api = new ApiHost();
        GetEmployeesRequest? sent = null;
        api.Db.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetEmployeesRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "1 employees found (page 2).",
                new PagedResponse<EmployeeModel>([SampleEmployee()], 51, 2, 50)));

        var response = await api.GetAsync(
            $"/api/employee/all?pageNumber=2&pageSize=50&sortColumn=email&sortDirection=desc&searchTerm=%20ana%20&officeId={OfficeId}");

        var page = (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data");
        Assert.Equal(51, page.GetProperty("totalItems").GetInt32());
        Assert.Equal(2, page.GetProperty("pageNumber").GetInt32());
        Assert.Equal(50, page.GetProperty("pageSize").GetInt32());
        Assert.Equal("Ana", page.GetProperty("items")[0].GetProperty("firstName").GetString());
        Assert.NotNull(sent);
        Assert.Equal(2, sent.PageNumber);
        Assert.Equal(50, sent.PageSize);
        Assert.Equal(EmployeeSortColumn.Email, sent.SortColumn);
        Assert.Equal(SortDirection.Desc, sent.SortDirection);
        Assert.Equal("ana", sent.SearchTerm);
        Assert.Equal(OfficeId, sent.OfficeId);
        Assert.Null(sent.DepartmentId);
    }

    [Fact]
    public async Task GetAll_ATooLargePage_IsA400Problem_WithoutTouchingTheDb()
    {
        await using var api = new ApiHost();

        var response = await api.GetAsync("/api/employee/all?pageNumber=1&pageSize=101");

        var problem = await ApiHost.ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal("Page size must be between 1 and 100.", problem.GetProperty("detail").GetString());
        api.Db.Verify(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetExport_ReturnsACsvFile()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.GetEmployeesAsync(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<EmployeeModel>>(200, "",
                new PagedResponse<EmployeeModel>([SampleEmployee()], 1, 1, 5000)));

        var response = await api.GetAsync("/api/employee/export?sortColumn=name&sortDirection=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Matches(@"^employees_\d{8}_\d{6}\.csv$", response.Content.Headers.ContentDisposition?.FileName);
        var csv = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Employee ID,First Name,Last Name", csv);
        Assert.Contains("ana@example.com", csv);
    }

    [Fact]
    public async Task PatchUpdate_SendsOnlyTheGivenFields_AndAuditsWhatChanged()
    {
        await using var api = new ApiHost();
        UpdateEmployeeRequest? sent = null;
        api.Db.Setup(d => d.UpdateEmployeeAsync(It.IsAny<UpdateEmployeeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<UpdateEmployeeRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(Ok());

        var response = await api.PatchAsync("/api/employee/update",
            new { employeeId = EmployeeId, firstName = "Ioana", officeId = OfficeId });

        await ApiHost.ReadEnvelopeAsync(response);
        Assert.NotNull(sent);
        Assert.Equal(EmployeeId, sent.EmployeeId);
        Assert.Equal("Ioana", sent.FirstName);
        Assert.Equal(OfficeId, sent.OfficeId);
        Assert.Null(sent.LastName);
        Assert.Null(sent.Address);
        api.VerifyAudit(EmployeeId, AuditAction.Edited, "Updated: first name, office");
    }

    [Fact]
    public async Task PatchDeactivate_DeactivatesThatEmployee_AndAuditsIt()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.DeactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(Ok());

        var response = await api.PatchAsync($"/api/employee/deactivate?employeeId={EmployeeId}");

        await ApiHost.ReadEnvelopeAsync(response);
        api.Db.Verify(d => d.ReactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        api.VerifyAudit(EmployeeId, AuditAction.Deactivated);
    }

    [Fact]
    public async Task PatchReactivate_ReactivatesThatEmployee_AndAuditsIt()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.ReactivateEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(Ok());

        var response = await api.PatchAsync($"/api/employee/reactivate?employeeId={EmployeeId}");

        await ApiHost.ReadEnvelopeAsync(response);
        api.Db.Verify(d => d.DeactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        api.VerifyAudit(EmployeeId, AuditAction.Reactivated);
    }

    [Fact]
    public async Task PatchDeactivate_WithoutAnEmployeeId_IsA400Problem_WithoutTouchingTheDb()
    {
        await using var api = new ApiHost();

        var response = await api.PatchAsync("/api/employee/deactivate");

        var problem = await ApiHost.ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal("Invalid or empty employee ID.", problem.GetProperty("detail").GetString());
        api.Db.Verify(d => d.DeactivateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteDelete_DeletesThatEmployee_AndAuditsIt()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.DeleteEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(Ok());

        var response = await api.DeleteAsync($"/api/employee/delete?employeeId={EmployeeId}");

        await ApiHost.ReadEnvelopeAsync(response);
        api.VerifyAudit(EmployeeId, AuditAction.Deleted);
    }

    [Fact]
    public async Task DeleteDelete_AStateConflictFromTheDb_IsA409Problem_AndNotAudited()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.DeleteEmployeeAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(409, "Only a deactivated employee can be deleted."));

        var response = await api.DeleteAsync($"/api/employee/delete?employeeId={EmployeeId}");

        var problem = await ApiHost.ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.Equal("Only a deactivated employee can be deleted.", problem.GetProperty("detail").GetString());
        api.Db.Verify(d => d.LogEmployeeAuditAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSalaryHistory_ReturnsTheEmployeesSalaries()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.GetEmployeeSalaryHistoryAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<SalaryModel>>(200, "", [
                new SalaryModel
                {
                    EmployeeSalaryId = 4, EmployeeId = EmployeeId, GrossSalary = 12000.5m,
                    EffectiveDate = new DateOnly(2026, 1, 1), CreatedAt = DateTime.UtcNow
                }
            ]));

        var response = await api.GetAsync($"/api/employee/salary-history?employeeId={EmployeeId}");

        var salary = (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data")[0];
        Assert.Equal(12000.5m, salary.GetProperty("grossSalary").GetDecimal());
        Assert.Equal("2026-01-01", salary.GetProperty("effectiveDate").GetString());
    }

    [Fact]
    public async Task PostSalaryHistory_SendsTheNewSalary_AndAuditsIt()
    {
        await using var api = new ApiHost();
        CreateSalaryRequest? sent = null;
        api.Db.Setup(d => d.CreateEmployeeSalaryAsync(It.IsAny<CreateSalaryRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateSalaryRequest, CancellationToken>((request, _) => sent = request)
            .ReturnsAsync(Ok());

        var response = await api.PostAsync("/api/employee/salary-history",
            new { employeeId = EmployeeId, grossSalary = 12500.75m, effectiveDate = "2026-02-01" });

        await ApiHost.ReadEnvelopeAsync(response);
        Assert.NotNull(sent);
        Assert.Equal(EmployeeId, sent.EmployeeId);
        Assert.Equal(12500.75m, sent.GrossSalary);
        Assert.Equal(new DateOnly(2026, 2, 1), sent.EffectiveDate);
        api.VerifyAudit(EmployeeId, AuditAction.SalaryChanged, "Gross salary set to 12500.75 effective 2026-02-01");
    }

    [Fact]
    public async Task GetAuditLog_ReturnsTheEmployeesEntries_WithTheActionAsText()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.GetEmployeeAuditLogAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<IReadOnlyList<AuditLogEntry>>(200, "", [
                new AuditLogEntry
                {
                    EmployeeAuditLogId = 7, EmployeeId = EmployeeId, PerformedBy = ApiHost.Username,
                    ActionType = AuditAction.SalaryChanged, OccurredAt = DateTime.UtcNow
                }
            ]));

        var response = await api.GetAsync($"/api/employee/audit-log?employeeId={EmployeeId}");

        var entry = (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data")[0];
        Assert.Equal(7, entry.GetProperty("employeeAuditLogId").GetInt32());
        Assert.Equal("SalaryChanged", entry.GetProperty("actionType").GetString());
        Assert.Equal(JsonValueKind.Null, entry.GetProperty("details").ValueKind);
    }

    [Fact]
    public async Task GetAllAuditLog_PassesThePage_AndReturnsIt()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.GetAllEmployeeAuditLogAsync(3, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(200, "",
                new PagedResponse<GlobalAuditLogEntry>([], 41, 3, 20)));

        var response = await api.GetAsync("/api/employee/audit-log/all?pageNumber=3");

        var page = (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data");
        Assert.Equal(41, page.GetProperty("totalItems").GetInt32());
        Assert.Equal(3, page.GetProperty("pageNumber").GetInt32());
    }

    [Fact]
    public async Task DeleteAllAuditLog_ClearsTheLog()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.DeleteAllEmployeeAuditLogAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Ok());

        var response = await api.DeleteAsync("/api/employee/audit-log/all");

        await ApiHost.ReadEnvelopeAsync(response);
        api.Db.Verify(d => d.DeleteAllEmployeeAuditLogAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetInsights_ReturnsTheProfilesWithTheirSalaryHistory()
    {
        await using var api = new ApiHost();
        api.Db.Setup(d => d.GetEmployeeInsightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<EmployeeInsightsModel>(200, "", new EmployeeInsightsModel
            {
                Employees =
                [
                    new EmployeeProfileModel
                    {
                        Status = EmployeeStatus.Active, Gender = Gender.Male, DepartmentName = "IT",
                        SalaryHistory = [new SalaryPointModel { EffectiveDate = new DateOnly(2026, 1, 1), GrossSalary = 9000m }],
                    }
                ],
            }));

        var response = await api.GetAsync("/api/employee/insights");

        var profile = (await ApiHost.ReadEnvelopeAsync(response)).GetProperty("data").GetProperty("employees")[0];
        Assert.Equal("IT", profile.GetProperty("departmentName").GetString());
        Assert.Equal(9000m, profile.GetProperty("salaryHistory")[0].GetProperty("grossSalary").GetDecimal());
    }
}
