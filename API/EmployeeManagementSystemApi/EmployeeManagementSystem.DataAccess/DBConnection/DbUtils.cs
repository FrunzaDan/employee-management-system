using System.Data;
using EmployeeManagementSystem.Domain.Configuration;
using EmployeeManagementSystem.Domain.Constants;
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
            "dbo.Employee_Create",
            command => DbHelper.AddEmployeeParametersForCreate(command, employee),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployee(GetEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_Get",
            // Employee_Get has one typed parameter per search kind; bind only the one that
            // EmployeeGetting detected, so the proc runs that branch's index seek.
            command =>
            {
                switch (request.SearchOption)
                {
                    case EmployeeSearchOption.Guid:
                        DbHelper.AddParameter(command, "@EmployeeId", SqlDbType.UniqueIdentifier,
                            Guid.Parse(request.SearchVariable!));
                        break;
                    case EmployeeSearchOption.Msisdn:
                        DbHelper.AddParameter(command, "@PhoneNumber", SqlDbType.VarChar, request.SearchVariable,
                            FieldLengthConstants.Msisdn);
                        break;
                    case EmployeeSearchOption.Email:
                        DbHelper.AddParameter(command, "@Email", SqlDbType.NVarChar, request.SearchVariable,
                            FieldLengthConstants.Email);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(request), request.SearchOption,
                            "No search option was detected for this request.");
                }
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
            "dbo.Employee_List",
            command =>
            {
                DbHelper.AddParameter(command, "@PageNumber", SqlDbType.Int, request.PageNumber);
                DbHelper.AddParameter(command, "@PageSize", SqlDbType.Int, request.PageSize);
                DbHelper.AddParameter(command, "@SearchTerm", SqlDbType.NVarChar, request.SearchTerm,
                    FieldLengthConstants.SearchTerm);
                DbHelper.AddParameter(command, "@SortColumn", SqlDbType.NVarChar, request.SortColumn, 20);
                DbHelper.AddParameter(command, "@SortDirection", SqlDbType.NVarChar, request.SortDirection, 4);
            },
            reader => DbHelper.HandleResponseWithPagedList(reader, request.PageNumber, request.PageSize, "employees"),
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditEmployee(EmployeeModel employee,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_Update",
            command => DbHelper.AddEmployeeParametersForEdit(command, employee),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeactivateEmployee(Guid employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_Deactivate",
            command => DbHelper.AddParameter(command, "@EmployeeId", SqlDbType.UniqueIdentifier, employeeGuid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> ReactivateEmployee(Guid employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_Reactivate",
            command => DbHelper.AddParameter(command, "@EmployeeId", SqlDbType.UniqueIdentifier, employeeGuid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteEmployee(Guid employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_Delete",
            command => DbHelper.AddParameter(command, "@EmployeeId", SqlDbType.UniqueIdentifier, employeeGuid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<int?>> CheckEmployerCredentialsFromDb(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default)
    {
        var authData = await ExecuteStoredProcedureAsync(
            "dbo.Employer_GetAuthData",
            command => DbHelper.AddParameter(command, "@Username", SqlDbType.NVarChar,
                employerCredentials.EmployerId, FieldLengthConstants.EmployerId),
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

    public async Task<ResponseModel<object>> LogEmployeeAudit(Guid employeeGuid, string employerId, string action,
        string? details, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_Create",
            command =>
            {
                DbHelper.AddParameter(command, "@EmployeeId", SqlDbType.UniqueIdentifier, employeeGuid);
                DbHelper.AddParameter(command, "@PerformedBy", SqlDbType.NVarChar, employerId,
                    FieldLengthConstants.EmployerId);
                DbHelper.AddParameter(command, "@ActionType", SqlDbType.VarChar, action,
                    FieldLengthConstants.AuditAction);
                DbHelper.AddParameter(command, "@Details", SqlDbType.NVarChar, details,
                    FieldLengthConstants.AuditDetails);
            },
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeeAuditLog(Guid employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_ListByEmployee",
            command => DbHelper.AddParameter(command, "@EmployeeId", SqlDbType.UniqueIdentifier, employeeGuid),
            DbHelper.HandleResponseWithAuditLogList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetAllEmployeeAuditLog(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_List",
            command =>
            {
                DbHelper.AddParameter(command, "@PageNumber", SqlDbType.Int, pageNumber);
                DbHelper.AddParameter(command, "@PageSize", SqlDbType.Int, pageSize);
            },
            reader => DbHelper.HandleResponseWithPagedAuditLogList(reader, pageNumber, pageSize),
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_DeleteAll",
            null,
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> AddEmployeeSalary(SalaryHistoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.EmployeeSalary_Create",
            command => DbHelper.AddSalaryParameters(command, entry),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeeSalaryHistory(Guid employeeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.EmployeeSalary_ListByEmployee",
            command => DbHelper.AddParameter(command, "@EmployeeId", SqlDbType.UniqueIdentifier, employeeGuid),
            DbHelper.HandleResponseWithSalaryHistoryList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> CreateOffice(OfficeModel office,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Office_Create",
            command => DbHelper.AddOfficeParameters(command, office),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetOffice(Guid guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Office_Get",
            command => DbHelper.AddParameter(command, "@OfficeId", SqlDbType.UniqueIdentifier, guid),
            DbHelper.HandleResponseWithOfficeMapping,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetOffices(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Office_List",
            null,
            DbHelper.HandleResponseWithOfficeList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditOffice(OfficeModel office,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Office_Update",
            command => DbHelper.AddOfficeParameters(command, office),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteOffice(Guid guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Office_Delete",
            command => DbHelper.AddParameter(command, "@OfficeId", SqlDbType.UniqueIdentifier, guid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeesByOffice(Guid officeGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByOffice",
            command => DbHelper.AddParameter(command, "@OfficeId", SqlDbType.UniqueIdentifier, officeGuid),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> CreateDepartment(DepartmentModel department,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Department_Create",
            command => DbHelper.AddDepartmentParameters(command, department),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetDepartment(Guid guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Department_Get",
            command => DbHelper.AddParameter(command, "@DepartmentId", SqlDbType.UniqueIdentifier, guid),
            DbHelper.HandleResponseWithDepartmentMapping,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetDepartments(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Department_List",
            null,
            DbHelper.HandleResponseWithDepartmentList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditDepartment(DepartmentModel department,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Department_Update",
            command => DbHelper.AddDepartmentParameters(command, department),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteDepartment(Guid guid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Department_Delete",
            command => DbHelper.AddParameter(command, "@DepartmentId", SqlDbType.UniqueIdentifier, guid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeesByDepartment(Guid departmentGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByDepartment",
            command => DbHelper.AddParameter(command, "@DepartmentId", SqlDbType.UniqueIdentifier, departmentGuid),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> CreateCostCenter(CostCenterModel costCenter,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Create",
            command => DbHelper.AddCostCenterParameters(command, costCenter),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetCostCenter(Guid guid, CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Get",
            command => DbHelper.AddParameter(command, "@CostCenterId", SqlDbType.UniqueIdentifier, guid),
            DbHelper.HandleResponseWithCostCenterMapping,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetCostCenters(CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CostCenter_List",
            null,
            DbHelper.HandleResponseWithCostCenterList,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> EditCostCenter(CostCenterModel costCenter,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Update",
            command => DbHelper.AddCostCenterParameters(command, costCenter),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> DeleteCostCenter(Guid guid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Delete",
            command => DbHelper.AddParameter(command, "@CostCenterId", SqlDbType.UniqueIdentifier, guid),
            DbHelper.HandleResponseWithMessage,
            cancellationToken
        );
    }

    public async Task<ResponseModel<object>> GetEmployeesByCostCenter(Guid costCenterGuid,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByCostCenter",
            command => DbHelper.AddParameter(command, "@CostCenterId", SqlDbType.UniqueIdentifier, costCenterGuid),
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
