using System.Data;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public class DbUtils(ISqlConnectionFactory connectionFactory) : IDbUtils
{
    public Task<ResponseModel<Guid?>> CreateEmployee(CreateEmployeeRequest employee,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Create",
            command => DbHelper.AddEmployeeParametersForCreate(command, employee),
            reader => DbHelper.HandleResponseWithCreatedGuid(reader, "EmployeeId"),
            cancellationToken);

    public Task<ResponseModel<EmployeeModel>> GetEmployee(EmployeeLookup lookup,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Get",
            command =>
            {
                command.Parameters.AddGuid("@EmployeeId", lookup.EmployeeId);
                command.Parameters.AddVarChar("@PhoneNumber", FieldLengthConstants.PhoneNumber, lookup.PhoneNumber);
                command.Parameters.AddNVarChar("@Email", FieldLengthConstants.Email, lookup.Email);
            },
            DbHelper.HandleResponseWithEmployee,
            cancellationToken);

    public Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployees(GetEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_List",
            command =>
            {
                command.Parameters.AddInt("@PageNumber", request.PageNumber);
                command.Parameters.AddInt("@PageSize", request.PageSize);
                command.Parameters.AddNVarChar("@SearchTerm", FieldLengthConstants.SearchTerm, request.SearchTerm);
                // The proc's CASE-based ORDER BY matches on the lowercase names ('name', 'asc', ...).
                command.Parameters.AddVarChar("@SortColumn", FieldLengthConstants.SortColumn,
                    request.SortColumn.ToString().ToLowerInvariant());
                command.Parameters.AddVarChar("@SortDirection", FieldLengthConstants.SortDirection,
                    request.SortDirection.ToString().ToLowerInvariant());
            },
            reader => DbHelper.HandleResponseWithPagedEmployees(reader, request.PageNumber, request.PageSize),
            cancellationToken);

    public Task<ResponseModel<object>> UpdateEmployee(UpdateEmployeeRequest employee,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Update",
            command => DbHelper.AddEmployeeParametersForUpdate(command, employee),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<object>> DeactivateEmployee(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Deactivate",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<object>> ReactivateEmployee(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Reactivate",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteEmployee(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Delete",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public async Task<ResponseModel<EmployerRole?>> CheckEmployerCredentialsFromDb(
        EmployerCredentials employerCredentials, CancellationToken cancellationToken = default)
    {
        var authData = await ExecuteStoredProcedureAsync(
            "dbo.Employer_GetAuthData",
            command => command.Parameters.AddNVarChar("@Username", FieldLengthConstants.Username,
                employerCredentials.Username),
            DbHelper.HandleEmployerAuthDataResponse,
            cancellationToken
        );

        if (authData is null ||
            !PasswordHasher.VerifyPassword(employerCredentials.Password ?? string.Empty, authData.PasswordHash,
                authData.PasswordSalt))
            return new ResponseModel<EmployerRole?>(401, "Invalid username or password.");

        var roleCode = (short)authData.EmployerRole;
        return authData.EmployerRole == EmployerRole.Employer
            ? new ResponseModel<EmployerRole?>(200, $"Credentials validated successfully. Role: {roleCode}.",
                authData.EmployerRole)
            : new ResponseModel<EmployerRole?>(403, $"The provided employer role ({roleCode}) is not valid.");
    }

    public Task<ResponseModel<object>> LogEmployeeAudit(Guid employeeId, string performedBy, AuditAction action,
        string? details, CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_Create",
            command =>
            {
                command.Parameters.AddGuid("@EmployeeId", employeeId);
                command.Parameters.AddNVarChar("@PerformedBy", FieldLengthConstants.Username, performedBy);
                command.Parameters.AddVarChar("@ActionType", FieldLengthConstants.AuditAction, action.ToString());
                command.Parameters.AddNVarChar("@Details", FieldLengthConstants.AuditDetails, details);
            },
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLog(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_ListByEmployee",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithAuditLogList,
            cancellationToken);

    public Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLog(int pageNumber,
        int pageSize, CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_List",
            command =>
            {
                command.Parameters.AddInt("@PageNumber", pageNumber);
                command.Parameters.AddInt("@PageSize", pageSize);
            },
            reader => DbHelper.HandleResponseWithPagedAuditLogList(reader, pageNumber, pageSize),
            cancellationToken);

    public Task<ResponseModel<object>> DeleteAllEmployeeAuditLog(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_DeleteAll",
            null,
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<object>> CreateEmployeeSalary(CreateSalaryRequest salary,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeSalary_Create",
            command => DbHelper.AddSalaryParametersForCreate(command, salary),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistory(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeSalary_ListByEmployee",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithSalaryList,
            cancellationToken);

    public Task<ResponseModel<Guid?>> CreateOffice(CreateOfficeRequest office,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Create",
            command => DbHelper.AddOfficeParametersForCreate(command, office),
            reader => DbHelper.HandleResponseWithCreatedGuid(reader, "OfficeId"),
            cancellationToken);

    public Task<ResponseModel<OfficeModel>> GetOffice(Guid officeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Get",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            DbHelper.HandleResponseWithOffice,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOffices(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_List",
            null,
            DbHelper.HandleResponseWithOfficeList,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateOffice(UpdateOfficeRequest office,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Update",
            command => DbHelper.AddOfficeParametersForUpdate(command, office),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteOffice(Guid officeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Delete",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOffice(Guid officeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByOffice",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken);

    public Task<ResponseModel<Guid?>> CreateDepartment(CreateDepartmentRequest department,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Create",
            command => DbHelper.AddDepartmentParametersForCreate(command, department),
            reader => DbHelper.HandleResponseWithCreatedGuid(reader, "DepartmentId"),
            cancellationToken);

    public Task<ResponseModel<DepartmentModel>> GetDepartment(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Get",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            DbHelper.HandleResponseWithDepartment,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartments(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_List",
            null,
            DbHelper.HandleResponseWithDepartmentList,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateDepartment(UpdateDepartmentRequest department,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Update",
            command => DbHelper.AddDepartmentParametersForUpdate(command, department),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteDepartment(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Delete",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartment(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByDepartment",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken);

    public Task<ResponseModel<Guid?>> CreateCostCenter(CreateCostCenterRequest costCenter,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Create",
            command => DbHelper.AddCostCenterParametersForCreate(command, costCenter),
            reader => DbHelper.HandleResponseWithCreatedGuid(reader, "CostCenterId"),
            cancellationToken);

    public Task<ResponseModel<CostCenterModel>> GetCostCenter(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Get",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            DbHelper.HandleResponseWithCostCenter,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCenters(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_List",
            null,
            DbHelper.HandleResponseWithCostCenterList,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateCostCenter(UpdateCostCenterRequest costCenter,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Update",
            command => DbHelper.AddCostCenterParametersForUpdate(command, costCenter),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteCostCenter(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Delete",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            DbHelper.HandleResponseWithMessage,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenter(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByCostCenter",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            DbHelper.HandleResponseWithEmployeeSummaryList,
            cancellationToken);

    private async Task<T> ExecuteStoredProcedureAsync<T>(
        string storedProcedure,
        Action<SqlCommand>? configureCommand,
        Func<SqlDataReader, Task<T>> handleReader,
        CancellationToken cancellationToken = default)
    {
        // No try/catch: expected outcomes come back as the proc's (Result, Message) row, and anything
        // thrown here (a SqlException the proc re-THROWs, a lost connection, a cancelled request)
        // propagates unchanged to GlobalExceptionHandler, which logs it once and answers 500.
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new SqlCommand(storedProcedure, connection);
        command.CommandType = CommandType.StoredProcedure;

        configureCommand?.Invoke(command);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await handleReader(reader);
    }
}
