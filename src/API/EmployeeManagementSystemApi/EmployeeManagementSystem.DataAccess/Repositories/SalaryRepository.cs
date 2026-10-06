using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.Repositories;

public class SalaryRepository(StoredProcedureExecutor executor) : ISalaryRepository
{
    public Task<ResponseModel<object>> CreateEmployeeSalaryAsync(CreateSalaryRequest salary,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.EmployeeSalary_Create",
            command => AddSalaryParametersForCreate(command, salary),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistoryAsync(Guid employeeId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.EmployeeSalary_ListByEmployee",
            command => command.Parameters.AddGuid("@EmployeeId", employeeId),
            HandleResponseWithSalaryListAsync,
            cancellationToken);

    private static void AddSalaryParametersForCreate(SqlCommand command, CreateSalaryRequest salary)
    {
        command.Parameters.AddGuid("@EmployeeId", salary.EmployeeId);
        command.Parameters.AddDecimal("@GrossSalary", 12, 2, salary.GrossSalary!.Value);
        command.Parameters.AddDate("@EffectiveDate", salary.EffectiveDate);
    }

    private static async Task<ResponseModel<IReadOnlyList<SalaryModel>>> HandleResponseWithSalaryListAsync(
        SqlDataReader reader)
    {
        var items = new List<SalaryModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapSalaryFromReader(reader));

        return new ResponseModel<IReadOnlyList<SalaryModel>>(200, $"{items.Count} salary history entries found.",
            items);
    }

    private static SalaryModel MapSalaryFromReader(SqlDataReader reader) => new()
    {
        EmployeeSalaryId = reader.GetInt32("EmployeeSalaryId"),
        EmployeeId = reader.GetGuid("EmployeeId"),
        GrossSalary = reader.GetDecimal("GrossSalary"),
        EffectiveDate = reader.GetDateOnly("EffectiveDate"),
        CreatedAt = reader.GetUtcDateTime("CreatedAt")
    };
}
