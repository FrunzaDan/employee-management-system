using System.Data;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

public class DbUtils(ISqlConnectionFactory connectionFactory) : IDbUtils
{
    public Task<ResponseModel<Guid?>> CreateEmployeeAsync(CreateEmployeeRequest employee,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Create",
            command => DbHelper.AddEmployeeParametersForCreate(command, employee),
            reader => DbHelper.HandleResponseWithCreatedGuidAsync(reader, "EmployeeId"),
            cancellationToken);

    public Task<ResponseModel<EmployeeModel>> GetEmployeeAsync(EmployeeLookup lookup,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Get",
            command =>
            {
                command.Parameters.AddGuid("@EmployeeId", lookup.EmployeeId);
                command.Parameters.AddVarChar("@PhoneNumber", FieldLengthConstants.PhoneNumber, lookup.PhoneNumber);
                command.Parameters.AddNVarChar("@Email", FieldLengthConstants.Email, lookup.Email);
            },
            DbHelper.HandleResponseWithEmployeeAsync,
            cancellationToken);

    public Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployeesAsync(GetEmployeesRequest request,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
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
            },
            reader => DbHelper.HandleResponseWithPagedEmployeesAsync(reader, request.PageNumber, request.PageSize),
            cancellationToken);

    public Task<ResponseModel<object>> UpdateEmployeeAsync(UpdateEmployeeRequest employee,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Update",
            command => DbHelper.AddEmployeeParametersForUpdate(command, employee),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeactivateEmployeeAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Deactivate",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> ReactivateEmployeeAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Reactivate",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteEmployeeAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_Delete",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public async Task<ResponseModel<EmployerRole?>> CheckEmployerCredentialsFromDbAsync(
        EmployerCredentials employerCredentials, CancellationToken cancellationToken = default)
    {
        var authData = await ExecuteStoredProcedureAsync(
            "dbo.Employer_GetAuthData",
            command => command.Parameters.AddNVarChar("@Username", FieldLengthConstants.Username,
                employerCredentials.Username),
            DbHelper.HandleEmployerAuthDataResponseAsync,
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

    public Task<ResponseModel<object>> LogEmployeeAuditAsync(Guid employeeId, string performedBy, AuditAction action,
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
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLogAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_ListByEmployee",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithAuditLogListAsync,
            cancellationToken);

    public Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLogAsync(int pageNumber,
        int pageSize, CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_List",
            command =>
            {
                command.Parameters.AddInt("@PageNumber", pageNumber);
                command.Parameters.AddInt("@PageSize", pageSize);
            },
            reader => DbHelper.HandleResponseWithPagedAuditLogListAsync(reader, pageNumber, pageSize),
            cancellationToken);

    public Task<ResponseModel<object>> DeleteAllEmployeeAuditLogAsync(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeAuditLog_DeleteAll",
            null,
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> CreateEmployeeSalaryAsync(CreateSalaryRequest salary,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeSalary_Create",
            command => DbHelper.AddSalaryParametersForCreate(command, salary),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistoryAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.EmployeeSalary_ListByEmployee",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            DbHelper.HandleResponseWithSalaryListAsync,
            cancellationToken);

    public Task<ResponseModel<Guid?>> CreateOfficeAsync(CreateOfficeRequest office,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Create",
            command => DbHelper.AddOfficeParametersForCreate(command, office),
            reader => DbHelper.HandleResponseWithCreatedGuidAsync(reader, "OfficeId"),
            cancellationToken);

    public Task<ResponseModel<OfficeModel>> GetOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Get",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            DbHelper.HandleResponseWithOfficeAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOfficesAsync(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_List",
            null,
            DbHelper.HandleResponseWithOfficeListAsync,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateOfficeAsync(UpdateOfficeRequest office,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Update",
            command => DbHelper.AddOfficeParametersForUpdate(command, office),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Office_Delete",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByOffice",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            DbHelper.HandleResponseWithEmployeeSummaryListAsync,
            cancellationToken);

    public Task<ResponseModel<Guid?>> CreateDepartmentAsync(CreateDepartmentRequest department,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Create",
            command => DbHelper.AddDepartmentParametersForCreate(command, department),
            reader => DbHelper.HandleResponseWithCreatedGuidAsync(reader, "DepartmentId"),
            cancellationToken);

    public Task<ResponseModel<DepartmentModel>> GetDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Get",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            DbHelper.HandleResponseWithDepartmentAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartmentsAsync(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_List",
            null,
            DbHelper.HandleResponseWithDepartmentListAsync,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateDepartmentAsync(UpdateDepartmentRequest department,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Update",
            command => DbHelper.AddDepartmentParametersForUpdate(command, department),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Department_Delete",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByDepartment",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            DbHelper.HandleResponseWithEmployeeSummaryListAsync,
            cancellationToken);

    public Task<ResponseModel<Guid?>> CreateCostCenterAsync(CreateCostCenterRequest costCenter,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Create",
            command => DbHelper.AddCostCenterParametersForCreate(command, costCenter),
            reader => DbHelper.HandleResponseWithCreatedGuidAsync(reader, "CostCenterId"),
            cancellationToken);

    public Task<ResponseModel<CostCenterModel>> GetCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Get",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            DbHelper.HandleResponseWithCostCenterAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCentersAsync(CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_List",
            null,
            DbHelper.HandleResponseWithCostCenterListAsync,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateCostCenterAsync(UpdateCostCenterRequest costCenter,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Update",
            command => DbHelper.AddCostCenterParametersForUpdate(command, costCenter),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.CostCenter_Delete",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            DbHelper.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        ExecuteStoredProcedureAsync(
            "dbo.Employee_ListByCostCenter",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            DbHelper.HandleResponseWithEmployeeSummaryListAsync,
            cancellationToken);

    private async Task<T> ExecuteStoredProcedureAsync<T>(
        string storedProcedure,
        Action<SqlCommand>? configureCommand,
        Func<SqlDataReader, Task<T>> handleReader,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new SqlCommand(storedProcedure, connection);
        command.CommandType = CommandType.StoredProcedure;

        configureCommand?.Invoke(command);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await handleReader(reader);
    }
}
