using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public sealed record EmployerAuthData(byte[] PasswordHash, byte[] PasswordSalt, EmployerRole EmployerRole);

public static class DbHelper
{
    public static void AddEmployeeParametersForCreate(SqlCommand command, CreateEmployeeRequest employee)
    {
        AddEmployeeCoreParameters(command, employee.FirstName, employee.LastName, employee.Email,
            employee.PhoneNumber, employee.Gender ?? Gender.NotDeclared, employee.BirthDate);
        command.Parameters.AddSmallInt("@StatusCode", (short)(employee.Status ?? EmployeeStatus.Active));
        AddAddressParameters(command, employee.Address);
        AddJobInfoParameters(command, employee.HireDate, employee.OfficeId, employee.DepartmentId,
            employee.CostCenterId);
    }

    public static void AddEmployeeParametersForUpdate(SqlCommand command, UpdateEmployeeRequest employee)
    {
        command.Parameters.AddGuid("@EmployeeId", employee.EmployeeId);
        AddEmployeeCoreParameters(command, employee.FirstName, employee.LastName, employee.Email,
            employee.PhoneNumber, employee.Gender, employee.BirthDate);
        AddAddressParameters(command, employee.Address);
        AddJobInfoParameters(command, employee.HireDate, employee.OfficeId, employee.DepartmentId,
            employee.CostCenterId);
    }

    public static void AddSalaryParametersForCreate(SqlCommand command, CreateSalaryRequest salary)
    {
        command.Parameters.AddGuid("@EmployeeId", salary.EmployeeId);
        command.Parameters.AddDecimal("@GrossSalary", 12, 2, salary.GrossSalary!.Value);
        command.Parameters.AddDate("@EffectiveDate", salary.EffectiveDate);
    }

    public static void AddOfficeParametersForCreate(SqlCommand command, CreateOfficeRequest office)
    {
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.OfficeName, office.Name);
        command.Parameters.AddNVarChar("@City", FieldLengthConstants.City, office.City);
        command.Parameters.AddNVarChar("@Country", FieldLengthConstants.Country, office.Country);
    }

    public static void AddOfficeParametersForUpdate(SqlCommand command, UpdateOfficeRequest office)
    {
        command.Parameters.AddGuid("@OfficeId", office.OfficeId);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.OfficeName, office.Name);
        command.Parameters.AddNVarChar("@City", FieldLengthConstants.City, office.City);
        command.Parameters.AddNVarChar("@Country", FieldLengthConstants.Country, office.Country);
    }

    public static void AddDepartmentParametersForCreate(SqlCommand command, CreateDepartmentRequest department) =>
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.DepartmentName, department.Name);

    public static void AddDepartmentParametersForUpdate(SqlCommand command, UpdateDepartmentRequest department)
    {
        command.Parameters.AddGuid("@DepartmentId", department.DepartmentId);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.DepartmentName, department.Name);
    }

    public static void AddCostCenterParametersForCreate(SqlCommand command, CreateCostCenterRequest costCenter)
    {
        command.Parameters.AddNVarChar("@Code", FieldLengthConstants.CostCenterCode, costCenter.Code);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.CostCenterName, costCenter.Name);
    }

    public static void AddCostCenterParametersForUpdate(SqlCommand command, UpdateCostCenterRequest costCenter)
    {
        command.Parameters.AddGuid("@CostCenterId", costCenter.CostCenterId);
        command.Parameters.AddNVarChar("@Code", FieldLengthConstants.CostCenterCode, costCenter.Code);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.CostCenterName, costCenter.Name);
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

    public static async Task<ResponseModel<EmployeeModel>> HandleResponseWithEmployeeAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<EmployeeModel>(404, "Employee not found.");

        return new ResponseModel<EmployeeModel>(200, "Employee found.", MapEmployeeFromReader(reader));
    }

    public static async Task<ResponseModel<PagedResponse<EmployeeModel>>> HandleResponseWithPagedEmployeesAsync(
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

    public static async Task<ResponseModel<object>> HandleResponseWithMessageAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            throw new InvalidOperationException("The stored procedure returned no (Result, Message) row.");

        var result = reader.GetInt32("Result");
        var message = reader.GetNullableString("Message");
        return result == 0
            ? new ResponseModel<object>(200, message ?? "Operation successful!")
            : new ResponseModel<object>(result, message ?? "Operation failed.");
    }

    public static async Task<ResponseModel<Guid?>> HandleResponseWithCreatedGuidAsync(SqlDataReader reader,
        string guidColumn)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            throw new InvalidOperationException("The stored procedure returned no (Result, Message) row.");

        var result = reader.GetInt32("Result");
        var message = reader.GetNullableString("Message");
        return result == 0
            ? new ResponseModel<Guid?>(200, message ?? "Operation successful!", reader.GetNullableGuid(guidColumn))
            : new ResponseModel<Guid?>(result, message ?? "Operation failed.");
    }

    public static async Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> HandleResponseWithAuditLogListAsync(
        SqlDataReader reader)
    {
        var items = new List<AuditLogEntry>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapAuditLogEntryFromReader(reader));

        return new ResponseModel<IReadOnlyList<AuditLogEntry>>(200, $"{items.Count} audit log entries found.", items);
    }

    public static async Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> HandleResponseWithPagedAuditLogListAsync(
        SqlDataReader reader, int pageNumber, int pageSize)
    {
        await reader.ReadAsync().ConfigureAwait(false);
        var totalItems = reader.GetInt32("TotalCount");

        var items = new List<GlobalAuditLogEntry>();
        await reader.NextResultAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapGlobalAuditLogEntryFromReader(reader));

        return new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(200,
            $"{items.Count} audit log entries found (page {pageNumber}).",
            new PagedResponse<GlobalAuditLogEntry>(items, totalItems, pageNumber, pageSize));
    }

    public static async Task<ResponseModel<IReadOnlyList<SalaryModel>>> HandleResponseWithSalaryListAsync(
        SqlDataReader reader)
    {
        var items = new List<SalaryModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapSalaryFromReader(reader));

        return new ResponseModel<IReadOnlyList<SalaryModel>>(200, $"{items.Count} salary history entries found.",
            items);
    }

    public static async Task<ResponseModel<OfficeModel>> HandleResponseWithOfficeAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<OfficeModel>(404, "Office not found.");

        return new ResponseModel<OfficeModel>(200, "Office found.", MapOfficeFromReader(reader));
    }

    public static async Task<ResponseModel<IReadOnlyList<OfficeModel>>> HandleResponseWithOfficeListAsync(
        SqlDataReader reader)
    {
        var items = new List<OfficeModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapOfficeFromReader(reader) with
            {
                EmployeeCount = reader.GetInt32("EmployeeCount"),
                TotalGrossSalary = reader.GetDecimal("TotalGrossSalary")
            });

        return new ResponseModel<IReadOnlyList<OfficeModel>>(200, $"{items.Count} offices found.", items);
    }

    public static async Task<ResponseModel<DepartmentModel>> HandleResponseWithDepartmentAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<DepartmentModel>(404, "Department not found.");

        return new ResponseModel<DepartmentModel>(200, "Department found.", MapDepartmentFromReader(reader));
    }

    public static async Task<ResponseModel<IReadOnlyList<DepartmentModel>>> HandleResponseWithDepartmentListAsync(
        SqlDataReader reader)
    {
        var items = new List<DepartmentModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapDepartmentFromReader(reader) with
            {
                EmployeeCount = reader.GetInt32("EmployeeCount"),
                TotalGrossSalary = reader.GetDecimal("TotalGrossSalary")
            });

        return new ResponseModel<IReadOnlyList<DepartmentModel>>(200, $"{items.Count} departments found.", items);
    }

    public static async Task<ResponseModel<CostCenterModel>> HandleResponseWithCostCenterAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<CostCenterModel>(404, "Cost center not found.");

        return new ResponseModel<CostCenterModel>(200, "Cost center found.", MapCostCenterFromReader(reader));
    }

    public static async Task<ResponseModel<IReadOnlyList<CostCenterModel>>> HandleResponseWithCostCenterListAsync(
        SqlDataReader reader)
    {
        var items = new List<CostCenterModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapCostCenterFromReader(reader) with
            {
                EmployeeCount = reader.GetInt32("EmployeeCount"),
                TotalGrossSalary = reader.GetDecimal("TotalGrossSalary")
            });

        return new ResponseModel<IReadOnlyList<CostCenterModel>>(200, $"{items.Count} cost centers found.", items);
    }

    public static async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> HandleResponseWithEmployeeSummaryListAsync(
        SqlDataReader reader)
    {
        var items = new List<EmployeeSummaryModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapEmployeeSummaryFromReader(reader));

        return new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(200, $"{items.Count} employees found.", items);
    }

    public static async Task<EmployerAuthData?> HandleEmployerAuthDataResponseAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false)) return null;

        return new EmployerAuthData(
            reader.GetBytes("PasswordHash"),
            reader.GetBytes("PasswordSalt"),
            (EmployerRole)reader.GetInt16("RoleCode")
        );
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
        CreatedAt = reader.GetUtcDateTime("CreatedAt"),
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

    private static SalaryModel MapSalaryFromReader(SqlDataReader reader) => new()
    {
        EmployeeSalaryId = reader.GetInt32("EmployeeSalaryId"),
        EmployeeId = reader.GetGuid("EmployeeId"),
        GrossSalary = reader.GetDecimal("GrossSalary"),
        EffectiveDate = reader.GetDateOnly("EffectiveDate"),
        CreatedAt = reader.GetUtcDateTime("CreatedAt")
    };

    private static OfficeModel MapOfficeFromReader(SqlDataReader reader) => new()
    {
        OfficeId = reader.GetGuid("OfficeId"),
        Name = reader.GetString("Name"),
        City = reader.GetNullableString("City"),
        Country = reader.GetNullableString("Country")
    };

    private static DepartmentModel MapDepartmentFromReader(SqlDataReader reader) => new()
    {
        DepartmentId = reader.GetGuid("DepartmentId"),
        Name = reader.GetString("Name")
    };

    private static CostCenterModel MapCostCenterFromReader(SqlDataReader reader) => new()
    {
        CostCenterId = reader.GetGuid("CostCenterId"),
        Code = reader.GetString("Code"),
        Name = reader.GetNullableString("Name")
    };

    private static EmployeeSummaryModel MapEmployeeSummaryFromReader(SqlDataReader reader) => new()
    {
        EmployeeId = reader.GetGuid("EmployeeId"),
        FirstName = reader.GetString("FirstName"),
        LastName = reader.GetString("LastName"),
        Email = reader.GetString("Email"),
        Status = (EmployeeStatus)reader.GetInt16("StatusCode")
    };

    private static AuditLogEntry MapAuditLogEntryFromReader(SqlDataReader reader) => new()
    {
        EmployeeAuditLogId = reader.GetInt32("EmployeeAuditLogId"),
        EmployeeId = reader.GetGuid("EmployeeId"),
        PerformedBy = reader.GetString("PerformedBy"),
        ActionType = Enum.Parse<AuditAction>(reader.GetString("ActionType")),
        Details = reader.GetNullableString("Details"),
        OccurredAt = reader.GetUtcDateTime("OccurredAt")
    };

    private static GlobalAuditLogEntry MapGlobalAuditLogEntryFromReader(SqlDataReader reader) => new()
    {
        EmployeeAuditLogId = reader.GetInt32("EmployeeAuditLogId"),
        EmployeeId = reader.GetGuid("EmployeeId"),
        EmployeeFirstName = reader.GetNullableString("FirstName"),
        EmployeeLastName = reader.GetNullableString("LastName"),
        PerformedBy = reader.GetString("PerformedBy"),
        ActionType = Enum.Parse<AuditAction>(reader.GetString("ActionType")),
        Details = reader.GetNullableString("Details"),
        OccurredAt = reader.GetUtcDateTime("OccurredAt")
    };
}
