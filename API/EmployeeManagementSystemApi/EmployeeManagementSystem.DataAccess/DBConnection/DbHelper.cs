using System.Data;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public sealed record EmployerAuthData(byte[] PasswordHash, byte[] PasswordSalt, int? EmployerRole);

public static class DbHelper
{
    // usp_createEmployee takes @var_EmployeeStatus; usp_editEmployee does not (status is
    // only ever changed via deactivate/reactivate) — so create and edit need separate
    // parameter sets, not one shared method that adds a parameter edit's proc doesn't declare.
    public static void AddEmployeeParametersForCreate(SqlCommand command, EmployeeModel employee)
    {
        AddEmployeeCoreParameters(command, employee);
        command.Parameters.AddWithValue("@var_EmployeeStatus", employee.EmployeeStatus ?? EmployeeStatusCodes.Active);
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
        command.Parameters.AddWithValue("@var_HireDate", (object?)employee.HireDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@var_OfficeGuid", (object?)employee.OfficeGuid ?? DBNull.Value);
        command.Parameters.AddWithValue("@var_DepartmentGuid", (object?)employee.DepartmentGuid ?? DBNull.Value);
        command.Parameters.AddWithValue("@var_CostCenterGuid", (object?)employee.CostCenterGuid ?? DBNull.Value);
    }

    public static void AddSalaryParameters(SqlCommand command, SalaryHistoryEntry entry)
    {
        command.Parameters.AddWithValue("@var_SalaryGuid", entry.SalaryGuid);
        command.Parameters.AddWithValue("@var_EmployeeGuid", entry.EmployeeGuid);
        command.Parameters.AddWithValue("@var_BruttoSalary", (object?)entry.BruttoSalary ?? DBNull.Value);
        command.Parameters.AddWithValue("@var_EffectiveDate", (object?)entry.EffectiveDate ?? DBNull.Value);
    }

    public static void AddOfficeParameters(SqlCommand command, OfficeModel office)
    {
        command.Parameters.AddWithValue("@var_Guid", office.Guid);
        command.Parameters.AddWithValue("@var_OfficeName", (object?)office.OfficeName ?? DBNull.Value);
        command.Parameters.AddWithValue("@var_City", (object?)office.City ?? DBNull.Value);
        command.Parameters.AddWithValue("@var_Country", (object?)office.Country ?? DBNull.Value);
    }

    public static void AddDepartmentParameters(SqlCommand command, DepartmentModel department)
    {
        command.Parameters.AddWithValue("@var_Guid", department.Guid);
        command.Parameters.AddWithValue("@var_DepartmentName", (object?)department.DepartmentName ?? DBNull.Value);
    }

    public static void AddCostCenterParameters(SqlCommand command, CostCenterModel costCenter)
    {
        command.Parameters.AddWithValue("@var_Guid", costCenter.Guid);
        command.Parameters.AddWithValue("@var_CostCenterCode", (object?)costCenter.CostCenterCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@var_CostCenterName", (object?)costCenter.CostCenterName ?? DBNull.Value);
    }

    private static void AddEmployeeCoreParameters(SqlCommand command, EmployeeModel employee)
    {
        command.Parameters.AddWithValue("@var_Guid", employee.Guid);
        command.Parameters.AddWithValue("@var_FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@var_LastName", employee.LastName);
        command.Parameters.AddWithValue("@var_Email", employee.Email);
        command.Parameters.AddWithValue("@var_MSISDN", employee.Msisdn);
        command.Parameters.Add("@var_Gender", SqlDbType.Int).Value = (object?)employee.Gender ?? DBNull.Value;
        command.Parameters.AddWithValue("@var_Birthdate", employee.Birthdate);
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
                totalItems = Convert.ToInt32(reader["total_count"]);

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
                totalItems = Convert.ToInt32(reader["total_count"]);

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

        if (await reader.IsDBNullAsync(reader.GetOrdinal("password_hash")).ConfigureAwait(false) ||
            await reader.IsDBNullAsync(reader.GetOrdinal("password_salt")).ConfigureAwait(false))
            return null;

        return new EmployerAuthData(
            (byte[])reader["password_hash"],
            (byte[])reader["password_salt"],
            reader["employer_role"] as int?
        );
    }

    // A real DB NULL must come back as a C# null, not "" — reader["col"].ToString() would
    // call DBNull.Value.ToString(), silently turning "never set" into "set to empty string".
    private static string? GetNullableString(SqlDataReader reader, string columnName) =>
        reader[columnName] as string;

    private static EmployeeModel MapEmployeeFromReader(SqlDataReader reader)
    {
        var employee = new EmployeeModel
        {
            Guid = GetNullableString(reader, "PK_employee_guid"),
            FirstName = GetNullableString(reader, "first_name"),
            LastName = GetNullableString(reader, "last_name"),
            Email = GetNullableString(reader, "email"),
            Msisdn = GetNullableString(reader, "msisdn"),
            CreationDate = GetNullableString(reader, "creation_Date"),
            InteractionDate = GetNullableString(reader, "interaction_Date"),
            Birthdate = GetNullableString(reader, "birthDate"),
            Address = new AddressModel
            {
                Country = GetNullableString(reader, "country"),
                County = GetNullableString(reader, "county"),
                Town = GetNullableString(reader, "town"),
                Zip = GetNullableString(reader, "zip_code"),
                Street = GetNullableString(reader, "street"),
                Number = GetNullableString(reader, "number")
            },
            Gender = int.TryParse(reader["gender"].ToString(), out var gender) ? gender : null,
            EmployeeStatus = int.TryParse(reader["employee_Status"].ToString(), out var status) ? status : null,
            HireDate = GetNullableString(reader, "hire_Date"),
            OfficeGuid = GetNullableString(reader, "PK_office_guid"),
            OfficeName = GetNullableString(reader, "office_name"),
            DepartmentGuid = GetNullableString(reader, "PK_department_guid"),
            DepartmentName = GetNullableString(reader, "department_name"),
            CostCenterGuid = GetNullableString(reader, "PK_cost_center_guid"),
            CostCenterName = GetNullableString(reader, "cost_center_name"),
            CurrentBruttoSalary = reader["current_brutto_salary"] as decimal?
        };

        return employee;
    }

    private static SalaryHistoryEntry MapSalaryHistoryEntryFromReader(SqlDataReader reader)
    {
        return new SalaryHistoryEntry
        {
            SalaryGuid = GetNullableString(reader, "PK_salary_guid"),
            EmployeeGuid = GetNullableString(reader, "FK_employee_guid"),
            BruttoSalary = reader["brutto_salary"] as decimal?,
            EffectiveDate = GetNullableString(reader, "effective_Date"),
            CreatedDate = GetNullableString(reader, "created_Date")
        };
    }

    private static OfficeModel MapOfficeFromReader(SqlDataReader reader)
    {
        return new OfficeModel
        {
            Guid = GetNullableString(reader, "PK_office_guid"),
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
        office.EmployeeCount = Convert.ToInt32(reader["employee_count"]);
        office.TotalBruttoSalary = Convert.ToDecimal(reader["total_brutto_salary"]);
        return office;
    }

    private static DepartmentModel MapDepartmentFromReader(SqlDataReader reader)
    {
        return new DepartmentModel
        {
            Guid = GetNullableString(reader, "PK_department_guid"),
            DepartmentName = GetNullableString(reader, "department_name")
        };
    }

    private static DepartmentModel MapDepartmentListItemFromReader(SqlDataReader reader)
    {
        var department = MapDepartmentFromReader(reader);
        department.EmployeeCount = Convert.ToInt32(reader["employee_count"]);
        department.TotalBruttoSalary = Convert.ToDecimal(reader["total_brutto_salary"]);
        return department;
    }

    private static CostCenterModel MapCostCenterFromReader(SqlDataReader reader)
    {
        return new CostCenterModel
        {
            Guid = GetNullableString(reader, "PK_cost_center_guid"),
            CostCenterCode = GetNullableString(reader, "cost_center_code"),
            CostCenterName = GetNullableString(reader, "cost_center_name")
        };
    }

    private static CostCenterModel MapCostCenterListItemFromReader(SqlDataReader reader)
    {
        var costCenter = MapCostCenterFromReader(reader);
        costCenter.EmployeeCount = Convert.ToInt32(reader["employee_count"]);
        costCenter.TotalBruttoSalary = Convert.ToDecimal(reader["total_brutto_salary"]);
        return costCenter;
    }

    private static EmployeeSummary MapEmployeeSummaryFromReader(SqlDataReader reader)
    {
        return new EmployeeSummary
        {
            Guid = GetNullableString(reader, "PK_employee_guid"),
            FirstName = GetNullableString(reader, "first_name"),
            LastName = GetNullableString(reader, "last_name"),
            Email = GetNullableString(reader, "email"),
            EmployeeStatus = int.TryParse(reader["employee_Status"].ToString(), out var status) ? status : null
        };
    }

    private static AuditLogEntry MapAuditLogEntryFromReader(SqlDataReader reader)
    {
        return new AuditLogEntry
        {
            AuditId = Convert.ToInt32(reader["audit_id"]),
            EmployeeGuid = GetNullableString(reader, "employee_guid"),
            EmployerId = GetNullableString(reader, "employer_id"),
            Action = GetNullableString(reader, "action"),
            Details = GetNullableString(reader, "details"),
            ActionDate = (DateTime)reader["action_Date"]
        };
    }

    private static GlobalAuditLogEntry MapGlobalAuditLogEntryFromReader(SqlDataReader reader)
    {
        return new GlobalAuditLogEntry
        {
            AuditId = Convert.ToInt32(reader["audit_id"]),
            EmployeeGuid = GetNullableString(reader, "employee_guid"),
            // A DBNull here (deleted employee, via the proc's LEFT JOIN) must come back
            // as a real null — see GetNullableString above.
            EmployeeFirstName = GetNullableString(reader, "first_name"),
            EmployeeLastName = GetNullableString(reader, "last_name"),
            EmployerId = GetNullableString(reader, "employer_id"),
            Action = GetNullableString(reader, "action"),
            Details = GetNullableString(reader, "details"),
            ActionDate = (DateTime)reader["action_Date"]
        };
    }

    private static void AddAddressParameters(SqlCommand command, AddressModel? address)
    {
        if (address == null) return;

        command.Parameters.AddWithValue("@var_Country", address.Country ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@var_County", address.County ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@var_Town", address.Town ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@var_ZIP", address.Zip ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@var_Street", address.Street ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@var_Number", address.Number ?? (object)DBNull.Value);
    }
}