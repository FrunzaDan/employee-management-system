using System.Data;
using EmployeeManagementSystem.Domain.Configuration;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public class DbUtils(IAppSettingsConfig configuration) : IDbUtils
{
    // DbUtils is registered as a singleton, so this cache is shared across every concurrent
    // request for the app's lifetime — the lock stops concurrent cold-start (or sustained
    // DB-unavailability) requests from redundantly re-running connection-string resolution.
    private readonly SemaphoreSlim _connectionStringLock = new(1, 1);
    private string? CurrentConnectionString { get; set; }

    public async Task<ResponseModel<object>> RegisterEmployee(EmployeeModel employee,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_createEmployee",
            command => DbHelper.AddEmployeeParametersForCreate(command, employee),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployee(GetEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployee",
            command =>
            {
                command.Parameters.Add("@var_SearchOption", SqlDbType.Int).Value = request.SearchOption;
                command.Parameters.AddWithValue("@var_SearchVariable", request.SearchVariable ?? (object)DBNull.Value);
            },
            reader => DbHelper.HandleResponseWithEmployeeMapping(reader, "Employee found.",
                "Employee not found"),
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployees(GetEmployeesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployees",
            command =>
            {
                command.Parameters.AddWithValue("@PageNumber", request.PageNumber);
                command.Parameters.AddWithValue("@PageSize", request.PageSize);
                command.Parameters.AddWithValue("@SearchTerm", (object?)request.SearchTerm ?? DBNull.Value);
                command.Parameters.AddWithValue("@SortColumn", request.SortColumn);
                command.Parameters.AddWithValue("@SortDirection", request.SortDirection);
            },
            reader => DbHelper.HandleResponseWithPagedList(reader, request.PageNumber, request.PageSize, "employees"),
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditEmployee(EmployeeModel employee,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_editEmployee",
            command => DbHelper.AddEmployeeParametersForEdit(command, employee),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeactivateEmployee(string employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_deactivateEmployee",
            command => command.Parameters.AddWithValue("@var_Guid", employeeGuid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> ReactivateEmployee(string employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_reactivateEmployee",
            command => command.Parameters.AddWithValue("@var_Guid", employeeGuid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteEmployee(string employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_deleteEmployee",
            command => command.Parameters.AddWithValue("@var_Guid", employeeGuid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<int?>> CheckEmployerCredentialsFromDb(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default)
    {
        var authData = await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployerAuthData",
            command => command.Parameters.AddWithValue("@var_EmployerID", employerCredentials.EmployerId),
            DbHelper.HandleEmployerAuthDataResponse,
            cancellationToken
        );

        if (authData is null ||
            !PasswordHasher.VerifyPassword(employerCredentials.EmployerPassword ?? string.Empty, authData.PasswordHash,
                authData.PasswordSalt))
            return new ResponseModel<int?>(403, "Invalid Employer ID or Password.");

        return authData.EmployerRole == 1801
            ? new ResponseModel<int?>(200, $"Credentials validated successfully. Role: {authData.EmployerRole}.",
                authData.EmployerRole)
            : new ResponseModel<int?>(403, $"The provided employer role ({authData.EmployerRole}) is not valid.");
    }

    public async Task<ResponseModel<object>> LogEmployeeAudit(string employeeGuid, string employerId, string action,
        string? details, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_insertEmployeeAuditLog",
            command =>
            {
                command.Parameters.AddWithValue("@var_EmployeeGuid", employeeGuid);
                command.Parameters.AddWithValue("@var_EmployerID", employerId);
                command.Parameters.AddWithValue("@var_Action", action);
                command.Parameters.AddWithValue("@var_Details", (object?)details ?? DBNull.Value);
            },
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeeAuditLog(string employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployeeAuditLog",
            command => command.Parameters.AddWithValue("@var_EmployeeGuid", employeeGuid),
            DbHelper.HandleResponseWithAuditLogList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetAllEmployeeAuditLog(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getAllEmployeeAuditLog",
            command =>
            {
                command.Parameters.AddWithValue("@PageNumber", pageNumber);
                command.Parameters.AddWithValue("@PageSize", pageSize);
            },
            reader => DbHelper.HandleResponseWithPagedAuditLogList(reader, pageNumber, pageSize),
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_deleteAllEmployeeAuditLog",
            null,
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> AddEmployeeSalary(SalaryHistoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_addEmployeeSalary",
            command => DbHelper.AddSalaryParameters(command, entry),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeeSalaryHistory(string employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployeeSalaryHistory",
            command => command.Parameters.AddWithValue("@var_EmployeeGuid", employeeGuid),
            DbHelper.HandleResponseWithSalaryHistoryList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> CreateOffice(OfficeModel office,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_createOffice",
            command => DbHelper.AddOfficeParameters(command, office),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetOffice(string guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getOffice",
            command => command.Parameters.AddWithValue("@var_Guid", guid),
            DbHelper.HandleResponseWithOfficeMapping,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetOffices(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getOffices",
            null,
            DbHelper.HandleResponseWithOfficeList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditOffice(OfficeModel office,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_editOffice",
            command => DbHelper.AddOfficeParameters(command, office),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteOffice(string guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_deleteOffice",
            command => command.Parameters.AddWithValue("@var_Guid", guid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeesByOffice(string officeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployeesByOffice",
            command => command.Parameters.AddWithValue("@var_Guid", officeGuid),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> CreateDepartment(DepartmentModel department,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_createDepartment",
            command => DbHelper.AddDepartmentParameters(command, department),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetDepartment(string guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getDepartment",
            command => command.Parameters.AddWithValue("@var_Guid", guid),
            DbHelper.HandleResponseWithDepartmentMapping,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetDepartments(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getDepartments",
            null,
            DbHelper.HandleResponseWithDepartmentList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditDepartment(DepartmentModel department,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_editDepartment",
            command => DbHelper.AddDepartmentParameters(command, department),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteDepartment(string guid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_deleteDepartment",
            command => command.Parameters.AddWithValue("@var_Guid", guid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeesByDepartment(string departmentGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployeesByDepartment",
            command => command.Parameters.AddWithValue("@var_Guid", departmentGuid),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> CreateCostCenter(CostCenterModel costCenter,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_createCostCenter",
            command => DbHelper.AddCostCenterParameters(command, costCenter),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetCostCenter(string guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getCostCenter",
            command => command.Parameters.AddWithValue("@var_Guid", guid),
            DbHelper.HandleResponseWithCostCenterMapping,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetCostCenters(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getCostCenters",
            null,
            DbHelper.HandleResponseWithCostCenterList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditCostCenter(CostCenterModel costCenter,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_editCostCenter",
            command => DbHelper.AddCostCenterParameters(command, costCenter),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteCostCenter(string guid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_deleteCostCenter",
            command => command.Parameters.AddWithValue("@var_Guid", guid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeesByCostCenter(string costCenterGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.usp_getEmployeesByCostCenter",
            command => command.Parameters.AddWithValue("@var_Guid", costCenterGuid),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken
        );
    }

    private async Task CheckConnectionStringAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(CurrentConnectionString)) return;

        await _connectionStringLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!string.IsNullOrEmpty(CurrentConnectionString)) return; // re-check after acquiring the lock

            CurrentConnectionString = await new CurrentSqlConnection(configuration)
                .GetCorrectSqlConnectionStringAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _connectionStringLock.Release();
        }
    }

    private async Task<T> ExecuteStoredProcedureAsync<T>(
        string storedProcedure,
        Action<SqlCommand>? configureCommand,
        Func<SqlDataReader, Task<T>> handleReader,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await CheckConnectionStringAsync(cancellationToken).ConfigureAwait(false);

            await using var connection = new SqlConnection(CurrentConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var command = new SqlCommand(storedProcedure, connection);
            command.CommandType = CommandType.StoredProcedure;

            configureCommand?.Invoke(command);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await handleReader(reader);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SqlException sqlEx)
        {
            throw new InvalidOperationException(
                $"Error executing stored procedure '{storedProcedure}': {sqlEx.Message}", sqlEx);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unexpected error during stored procedure execution: {ex.Message}",
                ex);
        }
    }
}
