using System.Data;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public sealed record EmployerAuthData(byte[] PasswordHash, byte[] PasswordSalt, short EmployerRole);

public static class DbHelper
{
    // usp_createEmployee takes @var_EmployeeStatus; usp_editEmployee does not (status is
    // only ever changed via deactivate/reactivate) — so create and edit need separate
    // parameter sets, not one shared method that adds a parameter edit's proc doesn't declare.
    public static void AddEmployeeParametersForCreate(SqlCommand command, EmployeeModel employee)
    {
        AddEmployeeCoreParameters(command, employee);
        AddParameter(command, "@var_EmployeeStatus", SqlDbType.SmallInt,
            (short)(employee.EmployeeStatus ?? EmployeeStatus.Active));
        AddAddressParameters(command, employee.Address);
        AddJobInfoParameters(command, employee);
    }

    public static void AddEmployeeParametersForEdit(SqlCommand command, EmployeeModel employee)
    {
        AddEmployeeCoreParameters(command, employee);
        AddAddressParameters(command, employee.Address);
        AddJobInfoParameters(command, employee);
    }

    // Shared by create and edit — usp_createEmployee/usp_editEmployee declare the same
    // four optional job-info parameters (see database.md).
    private static void AddJobInfoParameters(SqlCommand command, EmployeeModel employee)
    {
        AddParameter(command, "@var_HireDate", SqlDbType.Date, employee.HireDate);
        AddParameter(command, "@var_OfficeGuid", SqlDbType.UniqueIdentifier, employee.OfficeGuid);
        AddParameter(command, "@var_DepartmentGuid", SqlDbType.UniqueIdentifier, employee.DepartmentGuid);
        AddParameter(command, "@var_CostCenterGuid", SqlDbType.UniqueIdentifier, employee.CostCenterGuid);
    }

    public static void AddSalaryParameters(SqlCommand command, SalaryHistoryEntry entry)
    {
        AddParameter(command, "@var_SalaryGuid", SqlDbType.UniqueIdentifier, entry.SalaryGuid);
        AddParameter(command, "@var_EmployeeGuid", SqlDbType.UniqueIdentifier, entry.EmployeeGuid);
        var salary = AddParameter(command, "@var_BruttoSalary", SqlDbType.Decimal, entry.BruttoSalary);
        salary.Precision = 12;
        salary.Scale = 2;
        AddParameter(command, "@var_EffectiveDate", SqlDbType.Date, entry.EffectiveDate);
    }

    public static void AddOfficeParameters(SqlCommand command, OfficeModel office)
    {
        AddParameter(command, "@var_Guid", SqlDbType.UniqueIdentifier, office.Guid);
        AddParameter(command, "@var_OfficeName", SqlDbType.NVarChar, office.OfficeName, FieldLengthConstants.OfficeName);
        AddParameter(command, "@var_City", SqlDbType.NVarChar, office.City, FieldLengthConstants.City);
        AddParameter(command, "@var_Country", SqlDbType.NVarChar, office.Country, FieldLengthConstants.Country);
    }

    public static void AddDepartmentParameters(SqlCommand command, DepartmentModel department)
    {
        AddParameter(command, "@var_Guid", SqlDbType.UniqueIdentifier, department.Guid);
        AddParameter(command, "@var_DepartmentName", SqlDbType.NVarChar, department.DepartmentName,
            FieldLengthConstants.DepartmentName);
    }

    public static void AddCostCenterParameters(SqlCommand command, CostCenterModel costCenter)
    {
        AddParameter(command, "@var_Guid", SqlDbType.UniqueIdentifier, costCenter.Guid);
        AddParameter(command, "@var_CostCenterCode", SqlDbType.NVarChar, costCenter.CostCenterCode,
            FieldLengthConstants.CostCenterCode);
        AddParameter(command, "@var_CostCenterName", SqlDbType.NVarChar, costCenter.CostCenterName,
            FieldLengthConstants.CostCenterName);
    }

    private static void AddEmployeeCoreParameters(SqlCommand command, EmployeeModel employee)
    {
        AddParameter(command, "@var_Guid", SqlDbType.UniqueIdentifier, employee.Guid);
        AddParameter(command, "@var_FirstName", SqlDbType.NVarChar, employee.FirstName, FieldLengthConstants.FirstName);
        AddParameter(command, "@var_LastName", SqlDbType.NVarChar, employee.LastName, FieldLengthConstants.LastName);
        AddParameter(command, "@var_Email", SqlDbType.NVarChar, employee.Email, FieldLengthConstants.Email);
        AddParameter(command, "@var_MSISDN", SqlDbType.VarChar, employee.Msisdn, FieldLengthConstants.Msisdn);
        AddParameter(command, "@var_Gender", SqlDbType.TinyInt, (byte?)employee.Gender);
        AddParameter(command, "@var_Birthdate", SqlDbType.Date, employee.Birthdate);
    }

    // Every parameter is declared with its column's exact SQL type (and size), rather than
    // AddWithValue inferring one from the CLR value: a string would always go over the wire
    // as NVARCHAR(<length of this value>) — wrong for VARCHAR columns, and a different
    // declaration on every call — and a null would carry no type at all.
    internal static SqlParameter AddParameter(SqlCommand command, string name, SqlDbType type, object? value,
        int size = 0)
    {
        var parameter = command.Parameters.Add(name, type);
        if (size > 0)
            parameter.Size = size;
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }

    public static async Task<ResponseModel<object>> HandleResponseWithEmployeeMapping(SqlDataReader reader,
        string successMessage, string failureMessage)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<object>(404, failureMessage);

        return new ResponseModel<object>(200, successMessage, MapEmployeeFromReader(reader));
    }

    public static async Task<ResponseModel<object>> HandleResponseWithList(SqlDataReader reader, string entityName)
    {
        var items = new List<object>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapEmployeeFromReader(reader));

        return new ResponseModel<object>(200, $"{items.Count} {entityName} found.", items);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithPagedList(SqlDataReader reader,
        int pageNumber, int pageSize, string entityName)
    {
        var items = new List<EmployeeModel>();
        var totalItems = 0;

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            if (items.Count == 0)
                totalItems = reader.GetInt32(reader.GetOrdinal("total_count"));

            items.Add(MapEmployeeFromReader(reader));
        }

        var pagedResponse = new PagedResponse<EmployeeModel>(items, totalItems, pageNumber, pageSize);
        return new ResponseModel<object>(200, $"{items.Count} {entityName} found (page {pageNumber}).",
            pagedResponse);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithMessage(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<object>(500, "No data returned or operation failed.");

        var message = reader["message"] as string;
        return reader["result"] is 0
            ? new ResponseModel<object>(200, message ?? "Operation successful!")
            : new ResponseModel<object>(Convert.ToInt32(reader["result"]), message ?? "Operation failed.");
    }

    public static async Task<ResponseModel<object>> HandleResponseWithAuditLogList(SqlDataReader reader)
    {
        var items = new List<AuditLogEntry>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapAuditLogEntryFromReader(reader));

        return new ResponseModel<object>(200, $"{items.Count} audit log entries found.", items);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithPagedAuditLogList(SqlDataReader reader,
        int pageNumber, int pageSize)
    {
        var items = new List<GlobalAuditLogEntry>();
        var totalItems = 0;

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            if (items.Count == 0)
                totalItems = reader.GetInt32(reader.GetOrdinal("total_count"));

            items.Add(MapGlobalAuditLogEntryFromReader(reader));
        }

        var pagedResponse = new PagedResponse<GlobalAuditLogEntry>(items, totalItems, pageNumber, pageSize);
        return new ResponseModel<object>(200, $"{items.Count} audit log entries found (page {pageNumber}).",
            pagedResponse);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithSalaryHistoryList(SqlDataReader reader)
    {
        var items = new List<SalaryHistoryEntry>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapSalaryHistoryEntryFromReader(reader));

        return new ResponseModel<object>(200, $"{items.Count} salary history entries found.", items);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithOfficeMapping(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<object>(404, "Office not found.");

        return new ResponseModel<object>(200, "Office found.", MapOfficeFromReader(reader));
    }

    public static async Task<ResponseModel<object>> HandleResponseWithOfficeList(SqlDataReader reader)
    {
        var items = new List<OfficeModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapOfficeListItemFromReader(reader));

        return new ResponseModel<object>(200, $"{items.Count} offices found.", items);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithDepartmentMapping(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<object>(404, "Department not found.");

        return new ResponseModel<object>(200, "Department found.", MapDepartmentFromReader(reader));
    }

    public static async Task<ResponseModel<object>> HandleResponseWithDepartmentList(SqlDataReader reader)
    {
        var items = new List<DepartmentModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapDepartmentListItemFromReader(reader));

        return new ResponseModel<object>(200, $"{items.Count} departments found.", items);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithCostCenterMapping(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<object>(404, "Cost center not found.");

        return new ResponseModel<object>(200, "Cost center found.", MapCostCenterFromReader(reader));
    }

    public static async Task<ResponseModel<object>> HandleResponseWithCostCenterList(SqlDataReader reader)
    {
        var items = new List<CostCenterModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapCostCenterListItemFromReader(reader));

        return new ResponseModel<object>(200, $"{items.Count} cost centers found.", items);
    }

    public static async Task<ResponseModel<object>> HandleResponseWithEmployeeSummaryList(SqlDataReader reader)
    {
        var items = new List<EmployeeSummary>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapEmployeeSummaryFromReader(reader));

        return new ResponseModel<object>(200, $"{items.Count} employees found.", items);
    }

    public static async Task<EmployerAuthData?> HandleEmployerAuthDataResponse(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false)) return null;

        return new EmployerAuthData(
            reader.GetFieldValue<byte[]>(reader.GetOrdinal("password_hash")),
            reader.GetFieldValue<byte[]>(reader.GetOrdinal("password_salt")),
            reader.GetInt16(reader.GetOrdinal("employer_role"))
        );
    }

    // A real DB NULL must come back as a C# null, not "" — reader["col"].ToString() would
    // call DBNull.Value.ToString(), silently turning "never set" into "set to empty string".
    private static string? GetNullableString(SqlDataReader reader, string columnName) =>
        reader[columnName] as string;

    private static T? GetNullable<T>(SqlDataReader reader, string columnName) where T : struct
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T>(ordinal);
    }

    private static T Get<T>(SqlDataReader reader, string columnName) =>
        reader.GetFieldValue<T>(reader.GetOrdinal(columnName));

    // Every timestamp column is DATETIME2 holding UTC (SYSUTCDATETIME()), but SqlClient can't
    // know that and hands back DateTimeKind.Unspecified — which JSON-serializes with no zone
    // suffix, so a browser would misread it as local time. Mark it UTC so it goes out with "Z".
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static EmployeeModel MapEmployeeFromReader(SqlDataReader reader)
    {
        var employee = new EmployeeModel
        {
            Guid = Get<Guid>(reader, "PK_employee_guid"),
            FirstName = GetNullableString(reader, "first_name"),
            LastName = GetNullableString(reader, "last_name"),
            Email = GetNullableString(reader, "email"),
            Msisdn = GetNullableString(reader, "msisdn"),
            CreationDate = AsUtc(Get<DateTime>(reader, "creation_Date")),
            InteractionDate = AsUtc(Get<DateTime>(reader, "interaction_Date")),
            Birthdate = GetNullable<DateOnly>(reader, "birthdate"),
            Address = new AddressModel
            {
                Country = GetNullableString(reader, "country"),
                County = GetNullableString(reader, "county"),
                Town = GetNullableString(reader, "town"),
                Zip = GetNullableString(reader, "zip_code"),
                Street = GetNullableString(reader, "street"),
                Number = GetNullableString(reader, "number")
            },
            Gender = (Gender?)GetNullable<byte>(reader, "gender"),
            EmployeeStatus = (EmployeeStatus)Get<short>(reader, "employee_Status"),
            HireDate = GetNullable<DateOnly>(reader, "hire_Date"),
            OfficeGuid = GetNullable<Guid>(reader, "PK_office_guid"),
            OfficeName = GetNullableString(reader, "office_name"),
            DepartmentGuid = GetNullable<Guid>(reader, "PK_department_guid"),
            DepartmentName = GetNullableString(reader, "department_name"),
            CostCenterGuid = GetNullable<Guid>(reader, "PK_cost_center_guid"),
            CostCenterName = GetNullableString(reader, "cost_center_name"),
            CurrentBruttoSalary = GetNullable<decimal>(reader, "current_brutto_salary")
        };

        return employee;
    }

    private static SalaryHistoryEntry MapSalaryHistoryEntryFromReader(SqlDataReader reader)
    {
        return new SalaryHistoryEntry
        {
            SalaryGuid = Get<Guid>(reader, "PK_salary_guid"),
            EmployeeGuid = Get<Guid>(reader, "FK_employee_guid"),
            BruttoSalary = Get<decimal>(reader, "brutto_salary"),
            EffectiveDate = Get<DateOnly>(reader, "effective_Date"),
            CreatedDate = AsUtc(Get<DateTime>(reader, "created_Date"))
        };
    }

    private static OfficeModel MapOfficeFromReader(SqlDataReader reader)
    {
        return new OfficeModel
        {
            Guid = Get<Guid>(reader, "PK_office_guid"),
            OfficeName = GetNullableString(reader, "office_name"),
            City = GetNullableString(reader, "city"),
            Country = GetNullableString(reader, "country")
        };
    }

    // usp_getOffices (list) additionally aggregates employee_count/total_brutto_salary,
    // which the single-entity usp_getOffice doesn't compute — kept as a separate mapper
    // rather than making those columns optional on the shared one.
    private static OfficeModel MapOfficeListItemFromReader(SqlDataReader reader)
    {
        var office = MapOfficeFromReader(reader);
        office.EmployeeCount = Get<int>(reader, "employee_count");
        office.TotalBruttoSalary = Get<decimal>(reader, "total_brutto_salary");
        return office;
    }

    private static DepartmentModel MapDepartmentFromReader(SqlDataReader reader)
    {
        return new DepartmentModel
        {
            Guid = Get<Guid>(reader, "PK_department_guid"),
            DepartmentName = GetNullableString(reader, "department_name")
        };
    }

    private static DepartmentModel MapDepartmentListItemFromReader(SqlDataReader reader)
    {
        var department = MapDepartmentFromReader(reader);
        department.EmployeeCount = Get<int>(reader, "employee_count");
        department.TotalBruttoSalary = Get<decimal>(reader, "total_brutto_salary");
        return department;
    }

    private static CostCenterModel MapCostCenterFromReader(SqlDataReader reader)
    {
        return new CostCenterModel
        {
            Guid = Get<Guid>(reader, "PK_cost_center_guid"),
            CostCenterCode = GetNullableString(reader, "cost_center_code"),
            CostCenterName = GetNullableString(reader, "cost_center_name")
        };
    }

    private static CostCenterModel MapCostCenterListItemFromReader(SqlDataReader reader)
    {
        var costCenter = MapCostCenterFromReader(reader);
        costCenter.EmployeeCount = Get<int>(reader, "employee_count");
        costCenter.TotalBruttoSalary = Get<decimal>(reader, "total_brutto_salary");
        return costCenter;
    }

    private static EmployeeSummary MapEmployeeSummaryFromReader(SqlDataReader reader)
    {
        return new EmployeeSummary
        {
            Guid = Get<Guid>(reader, "PK_employee_guid"),
            FirstName = GetNullableString(reader, "first_name"),
            LastName = GetNullableString(reader, "last_name"),
            Email = GetNullableString(reader, "email"),
            EmployeeStatus = (EmployeeStatus)Get<short>(reader, "employee_Status")
        };
    }

    private static AuditLogEntry MapAuditLogEntryFromReader(SqlDataReader reader)
    {
        return new AuditLogEntry
        {
            AuditId = Get<int>(reader, "audit_id"),
            EmployeeGuid = Get<Guid>(reader, "employee_guid"),
            EmployerId = GetNullableString(reader, "employer_id"),
            Action = GetNullableString(reader, "action"),
            Details = GetNullableString(reader, "details"),
            ActionDate = AsUtc(Get<DateTime>(reader, "action_Date"))
        };
    }

    private static GlobalAuditLogEntry MapGlobalAuditLogEntryFromReader(SqlDataReader reader)
    {
        return new GlobalAuditLogEntry
        {
            AuditId = Get<int>(reader, "audit_id"),
            EmployeeGuid = Get<Guid>(reader, "employee_guid"),
            // A DBNull here (deleted employee, via the proc's LEFT JOIN) must come back
            // as a real null — see GetNullableString above.
            EmployeeFirstName = GetNullableString(reader, "first_name"),
            EmployeeLastName = GetNullableString(reader, "last_name"),
            EmployerId = GetNullableString(reader, "employer_id"),
            Action = GetNullableString(reader, "action"),
            Details = GetNullableString(reader, "details"),
            ActionDate = AsUtc(Get<DateTime>(reader, "action_Date"))
        };
    }

    private static void AddAddressParameters(SqlCommand command, AddressModel? address)
    {
        if (address == null) return;

        AddParameter(command, "@var_Country", SqlDbType.NVarChar, address.Country, FieldLengthConstants.Country);
        AddParameter(command, "@var_County", SqlDbType.NVarChar, address.County, FieldLengthConstants.County);
        AddParameter(command, "@var_Town", SqlDbType.NVarChar, address.Town, FieldLengthConstants.Town);
        AddParameter(command, "@var_ZIP", SqlDbType.VarChar, address.Zip, FieldLengthConstants.Zip);
        AddParameter(command, "@var_Street", SqlDbType.NVarChar, address.Street, FieldLengthConstants.Street);
        AddParameter(command, "@var_Number", SqlDbType.NVarChar, address.Number, FieldLengthConstants.Number);
    }
}