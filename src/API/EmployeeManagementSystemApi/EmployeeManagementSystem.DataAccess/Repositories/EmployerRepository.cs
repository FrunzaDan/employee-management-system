using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.Repositories;

public class EmployerRepository(StoredProcedureExecutor executor) : IEmployerRepository
{
    public Task<EmployerAuthData?> GetEmployerAuthDataAsync(string username,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employer_GetAuthData",
            command => command.Parameters.AddNVarChar("@Username", FieldLengthConstants.Username, username),
            HandleEmployerAuthDataResponseAsync,
            cancellationToken);

    public Task RecordEmployerLoginAsync(string username, CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employer_RecordLogin",
            command => command.Parameters.AddNVarChar("@Username", FieldLengthConstants.Username, username),
            _ => Task.FromResult(true),
            cancellationToken);

    private static async Task<EmployerAuthData?> HandleEmployerAuthDataResponseAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false)) return null;

        return new EmployerAuthData(
            reader.GetBytes("PasswordHash"),
            reader.GetBytes("PasswordSalt"),
            (EmployerRole)reader.GetInt16("RoleCode")
        );
    }
}
