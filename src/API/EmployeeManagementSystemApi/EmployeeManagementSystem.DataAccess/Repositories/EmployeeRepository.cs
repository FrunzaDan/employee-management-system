using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.Repositories;

public class EmployeeRepository(StoredProcedureExecutor executor) : IEmployeeRepository
{
    public Task<ResponseModel<Guid?>> CreateEmployeeAsync(CreateEmployeeRequest employee,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_Create",
            command => AddEmployeeParametersForCreate(command, employee),
            reader => StoredProcedureResults.HandleResponseWithCreatedGuidAsync(reader, "EmployeeId"),
            cancellationToken);

    public Task<ResponseModel<EmployeeModel>> GetEmployeeAsync(EmployeeLookup lookup,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_Get",
            command =>
            {
                command.Parameters.AddGuid("@EmployeeId", lookup.EmployeeId);
                command.Parameters.AddVarChar("@PhoneNumber", FieldLengthConstants.PhoneNumber, lookup.PhoneNumber);
                command.Parameters.AddNVarChar("@Email", FieldLengthConstants.Email, lookup.Email);
            },
            HandleResponseWithEmployeeAsync,
            cancellationToken);

    public Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployeesAsync(GetEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_List",
            command =>
            {
                command.Parameters.AddInt("@PageNumber", request.PageNumber);
                command.Parameters.AddInt("@PageSize", request.PageSize);
                command.Parameters.AddNVarChar("@SearchTerm", FieldLengthConstants.SearchTerm, request.SearchTerm);
                command.Parameters.AddVarChar("@SortColumn", FieldLengthConstants.SortColumn,
                    request.SortColumn.ToString().ToLowerInvariant());
                command.Parameters.AddVarChar("@SortDirection", FieldLengthConstants.SortDirection,
                    request.SortDirection.ToString().ToLowerInvariant());
                command.Parameters.AddGuid("@OfficeId", request.OfficeId);
                command.Parameters.AddGuid("@DepartmentId", request.DepartmentId);
                command.Parameters.AddGuid("@CostCenterId", request.CostCenterId);
            },
            reader => HandleResponseWithPagedEmployeesAsync(reader, request.PageNumber, request.PageSize),
            cancellationToken);

    public Task<ResponseModel<object>> UpdateEmployeeAsync(UpdateEmployeeRequest employee,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_Update",
            command => AddEmployeeParametersForUpdate(command, employee),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_Deactivate",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> ReactivateEmployeeAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_Reactivate",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteEmployeeAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_Delete",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<EmployeeInsightsModel>> GetEmployeeInsightsAsync(CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Report_GetEmployeeInsights",
            null,
            HandleResponseWithEmployeeInsightsAsync,
            cancellationToken);

    private static void AddEmployeeParametersForCreate(SqlCommand command, CreateEmployeeRequest employee)
    {
        AddEmployeeCoreParameters(command, employee.FirstName, employee.LastName, employee.Email,
            employee.PhoneNumber, employee.Gender ?? Gender.NotDeclared, employee.BirthDate);
        command.Parameters.AddSmallInt("@StatusCode", (short)(employee.Status ?? EmployeeStatus.Active));
        AddAddressParameters(command, employee.Address);
        AddJobInfoParameters(command, employee.HireDate, employee.OfficeId, employee.DepartmentId,
            employee.CostCenterId);
    }

    private static void AddEmployeeParametersForUpdate(SqlCommand command, UpdateEmployeeRequest employee)
    {
        command.Parameters.AddGuid("@EmployeeId", employee.EmployeeId);
        AddEmployeeCoreParameters(command, employee.FirstName, employee.LastName, employee.Email,
            employee.PhoneNumber, employee.Gender, employee.BirthDate);
        AddAddressParameters(command, employee.Address);
        AddJobInfoParameters(command, employee.HireDate, employee.OfficeId, employee.DepartmentId,
            employee.CostCenterId);
    }

    private static void AddEmployeeCoreParameters(SqlCommand command, string? firstName, string? lastName,
        string? email, string? phoneNumber, Gender? gender, DateOnly? birthDate)
    {
        command.Parameters.AddNVarChar("@FirstName", FieldLengthConstants.FirstName, firstName);
        command.Parameters.AddNVarChar("@LastName", FieldLengthConstants.LastName, lastName);
        command.Parameters.AddNVarChar("@Email", FieldLengthConstants.Email, email);
        command.Parameters.AddVarChar("@PhoneNumber", FieldLengthConstants.PhoneNumber, phoneNumber);
        command.Parameters.AddTinyInt("@Gender", (byte?)gender);
        command.Parameters.AddDate("@BirthDate", birthDate);
    }

    private static void AddAddressParameters(SqlCommand command, AddressRequest? address)
    {
        command.Parameters.AddNVarChar("@Country", FieldLengthConstants.Country, address?.Country);
        command.Parameters.AddNVarChar("@County", FieldLengthConstants.County, address?.County);
        command.Parameters.AddNVarChar("@City", FieldLengthConstants.City, address?.City);
        command.Parameters.AddVarChar("@PostalCode", FieldLengthConstants.PostalCode, address?.PostalCode);
        command.Parameters.AddNVarChar("@Street", FieldLengthConstants.Street, address?.Street);
        command.Parameters.AddNVarChar("@StreetNumber", FieldLengthConstants.StreetNumber, address?.StreetNumber);
    }

    private static void AddJobInfoParameters(SqlCommand command, DateOnly? hireDate, Guid? officeId,
        Guid? departmentId, Guid? costCenterId)
    {
        command.Parameters.AddDate("@HireDate", hireDate);
        command.Parameters.AddGuid("@OfficeId", officeId);
        command.Parameters.AddGuid("@DepartmentId", departmentId);
        command.Parameters.AddGuid("@CostCenterId", costCenterId);
    }

    private static async Task<ResponseModel<EmployeeModel>> HandleResponseWithEmployeeAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<EmployeeModel>(404, "Employee not found.");

        return new ResponseModel<EmployeeModel>(200, "Employee found.", MapEmployeeFromReader(reader));
    }

    private static async Task<ResponseModel<PagedResponse<EmployeeModel>>> HandleResponseWithPagedEmployeesAsync(
        SqlDataReader reader, int pageNumber, int pageSize)
    {
        await reader.ReadAsync().ConfigureAwait(false);
        var totalItems = reader.GetInt32("TotalCount");

        var items = new List<EmployeeModel>();
        await reader.NextResultAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapEmployeeFromReader(reader));

        return new ResponseModel<PagedResponse<EmployeeModel>>(200,
            $"{items.Count} employees found (page {pageNumber}).",
            new PagedResponse<EmployeeModel>(items, totalItems, pageNumber, pageSize));
    }

    private static async Task<ResponseModel<EmployeeInsightsModel>> HandleResponseWithEmployeeInsightsAsync(
        SqlDataReader reader)
    {
        var rows = new List<(Guid EmployeeId, EmployeeProfileModel Profile)>();
        while (await reader.ReadAsync().ConfigureAwait(false))
            rows.Add((reader.GetGuid("EmployeeId"), MapEmployeeProfileFromReader(reader)));

        // The id only joins the salary history to its employee; it isn't sent to the UI.
        var histories = new Dictionary<Guid, List<SalaryPointModel>>();
        await reader.NextResultAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var employeeId = reader.GetGuid("EmployeeId");
            if (!histories.TryGetValue(employeeId, out var history))
                histories[employeeId] = history = [];
            history.Add(new SalaryPointModel
            {
                EffectiveDate = reader.GetDateOnly("EffectiveDate"),
                GrossSalary = reader.GetDecimal("GrossSalary")
            });
        }

        var employees = rows
            .Select(row => histories.TryGetValue(row.EmployeeId, out var history)
                ? row.Profile with { SalaryHistory = history }
                : row.Profile)
            .ToList();

        return new ResponseModel<EmployeeInsightsModel>(200, "Employee insights retrieved.",
            new EmployeeInsightsModel { Employees = employees });
    }

    private static EmployeeModel MapEmployeeFromReader(SqlDataReader reader) => new()
    {
        EmployeeId = reader.GetGuid("EmployeeId"),
        FirstName = reader.GetString("FirstName"),
        LastName = reader.GetString("LastName"),
        Email = reader.GetString("Email"),
        PhoneNumber = reader.GetString("PhoneNumber"),
        Gender = (Gender)reader.GetByte("Gender"),
        BirthDate = reader.GetNullableDateOnly("BirthDate"),
        Status = (EmployeeStatus)reader.GetInt16("StatusCode"),
        AccountCreatedAt = reader.GetUtcDateTime("AccountCreatedAt"),
        LastInteractionAt = reader.GetUtcDateTime("LastInteractionAt"),
        Address = new AddressModel
        {
            Country = reader.GetString("Country"),
            County = reader.GetString("County"),
            City = reader.GetString("City"),
            PostalCode = reader.GetString("PostalCode"),
            Street = reader.GetString("Street"),
            StreetNumber = reader.GetString("StreetNumber")
        },
        HireDate = reader.GetNullableDateOnly("HireDate"),
        OfficeId = reader.GetNullableGuid("OfficeId"),
        OfficeName = reader.GetNullableString("OfficeName"),
        DepartmentId = reader.GetNullableGuid("DepartmentId"),
        DepartmentName = reader.GetNullableString("DepartmentName"),
        CostCenterId = reader.GetNullableGuid("CostCenterId"),
        CostCenterName = reader.GetNullableString("CostCenterName"),
        CurrentGrossSalary = reader.GetNullableDecimal("CurrentGrossSalary")
    };

    private static EmployeeProfileModel MapEmployeeProfileFromReader(SqlDataReader reader) => new()
    {
        Status = (EmployeeStatus)reader.GetInt16("StatusCode"),
        Gender = (Gender)reader.GetByte("Gender"),
        BirthDate = reader.GetNullableDateOnly("BirthDate"),
        HireDate = reader.GetNullableDateOnly("HireDate"),
        DepartmentName = reader.GetNullableString("DepartmentName"),
        OfficeName = reader.GetNullableString("OfficeName"),
        CurrentGrossSalary = reader.GetNullableDecimal("CurrentGrossSalary")
    };
}
