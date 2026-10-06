using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.Repositories;

public class DepartmentRepository(StoredProcedureExecutor executor) : IDepartmentRepository
{
    public Task<ResponseModel<Guid?>> CreateDepartmentAsync(CreateDepartmentRequest department,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Department_Create",
            command => AddDepartmentParametersForCreate(command, department),
            reader => StoredProcedureResults.HandleResponseWithCreatedGuidAsync(reader, "DepartmentId"),
            cancellationToken);

    public Task<ResponseModel<DepartmentModel>> GetDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Department_Get",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            HandleResponseWithDepartmentAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<DepartmentModel>>> GetDepartmentsAsync(CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Department_List",
            null,
            HandleResponseWithDepartmentListAsync,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateDepartmentAsync(UpdateDepartmentRequest department,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Department_Update",
            command => AddDepartmentParametersForUpdate(command, department),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Department_Delete",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_ListByDepartment",
            command => command.Parameters.AddGuid("@DepartmentId", departmentId),
            EmployeeSummaryResults.HandleResponseWithEmployeeSummaryListAsync,
            cancellationToken);

    private static void AddDepartmentParametersForCreate(SqlCommand command, CreateDepartmentRequest department) =>
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.DepartmentName, department.Name);

    private static void AddDepartmentParametersForUpdate(SqlCommand command, UpdateDepartmentRequest department)
    {
        command.Parameters.AddGuid("@DepartmentId", department.DepartmentId);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.DepartmentName, department.Name);
    }

    private static async Task<ResponseModel<DepartmentModel>> HandleResponseWithDepartmentAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<DepartmentModel>(404, "Department not found.");

        return new ResponseModel<DepartmentModel>(200, "Department found.", MapDepartmentFromReader(reader));
    }

    private static async Task<ResponseModel<IReadOnlyList<DepartmentModel>>> HandleResponseWithDepartmentListAsync(
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

    private static DepartmentModel MapDepartmentFromReader(SqlDataReader reader) => new()
    {
        DepartmentId = reader.GetGuid("DepartmentId"),
        Name = reader.GetString("Name")
    };
}
