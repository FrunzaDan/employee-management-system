using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.Repositories;

public class CostCenterRepository(StoredProcedureExecutor executor) : ICostCenterRepository
{
    public Task<ResponseModel<Guid?>> CreateCostCenterAsync(CreateCostCenterRequest costCenter,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.CostCenter_Create",
            command => AddCostCenterParametersForCreate(command, costCenter),
            reader => StoredProcedureResults.HandleResponseWithCreatedGuidAsync(reader, "CostCenterId"),
            cancellationToken);

    public Task<ResponseModel<CostCenterModel>> GetCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.CostCenter_Get",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            HandleResponseWithCostCenterAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<CostCenterModel>>> GetCostCentersAsync(CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.CostCenter_List",
            null,
            HandleResponseWithCostCenterListAsync,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateCostCenterAsync(UpdateCostCenterRequest costCenter,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.CostCenter_Update",
            command => AddCostCenterParametersForUpdate(command, costCenter),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.CostCenter_Delete",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByCostCenterAsync(Guid costCenterId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_ListByCostCenter",
            command => command.Parameters.AddGuid("@CostCenterId", costCenterId),
            EmployeeSummaryResults.HandleResponseWithEmployeeSummaryListAsync,
            cancellationToken);

    private static void AddCostCenterParametersForCreate(SqlCommand command, CreateCostCenterRequest costCenter)
    {
        command.Parameters.AddNVarChar("@Code", FieldLengthConstants.CostCenterCode, costCenter.Code);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.CostCenterName, costCenter.Name);
    }

    private static void AddCostCenterParametersForUpdate(SqlCommand command, UpdateCostCenterRequest costCenter)
    {
        command.Parameters.AddGuid("@CostCenterId", costCenter.CostCenterId);
        command.Parameters.AddNVarChar("@Code", FieldLengthConstants.CostCenterCode, costCenter.Code);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.CostCenterName, costCenter.Name);
    }

    private static async Task<ResponseModel<CostCenterModel>> HandleResponseWithCostCenterAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<CostCenterModel>(404, "Cost center not found.");

        return new ResponseModel<CostCenterModel>(200, "Cost center found.", MapCostCenterFromReader(reader));
    }

    private static async Task<ResponseModel<IReadOnlyList<CostCenterModel>>> HandleResponseWithCostCenterListAsync(
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

    private static CostCenterModel MapCostCenterFromReader(SqlDataReader reader) => new()
    {
        CostCenterId = reader.GetGuid("CostCenterId"),
        Code = reader.GetString("Code"),
        Name = reader.GetNullableString("Name")
    };
}
