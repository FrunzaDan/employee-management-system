using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.Repositories;

public class OfficeRepository(StoredProcedureExecutor executor) : IOfficeRepository
{
    public Task<ResponseModel<Guid?>> CreateOfficeAsync(CreateOfficeRequest office,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Office_Create",
            command => AddOfficeParametersForCreate(command, office),
            reader => StoredProcedureResults.HandleResponseWithCreatedGuidAsync(reader, "OfficeId"),
            cancellationToken);

    public Task<ResponseModel<OfficeModel>> GetOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Office_Get",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            HandleResponseWithOfficeAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<OfficeModel>>> GetOfficesAsync(CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Office_List",
            null,
            HandleResponseWithOfficeListAsync,
            cancellationToken);

    public Task<ResponseModel<object>> UpdateOfficeAsync(UpdateOfficeRequest office,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Office_Update",
            command => AddOfficeParametersForUpdate(command, office),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<object>> DeleteOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Office_Delete",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            StoredProcedureResults.HandleResponseWithMessageAsync,
            cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> GetEmployeesByOfficeAsync(Guid officeId,
        CancellationToken cancellationToken = default) =>
        executor.ExecuteAsync(
            "dbo.Employee_ListByOffice",
            command => command.Parameters.AddGuid("@OfficeId", officeId),
            EmployeeSummaryResults.HandleResponseWithEmployeeSummaryListAsync,
            cancellationToken);

    private static void AddOfficeParametersForCreate(SqlCommand command, CreateOfficeRequest office)
    {
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.OfficeName, office.Name);
        command.Parameters.AddNVarChar("@City", FieldLengthConstants.City, office.City);
        command.Parameters.AddNVarChar("@Country", FieldLengthConstants.Country, office.Country);
    }

    private static void AddOfficeParametersForUpdate(SqlCommand command, UpdateOfficeRequest office)
    {
        command.Parameters.AddGuid("@OfficeId", office.OfficeId);
        command.Parameters.AddNVarChar("@Name", FieldLengthConstants.OfficeName, office.Name);
        command.Parameters.AddNVarChar("@City", FieldLengthConstants.City, office.City);
        command.Parameters.AddNVarChar("@Country", FieldLengthConstants.Country, office.Country);
    }

    private static async Task<ResponseModel<OfficeModel>> HandleResponseWithOfficeAsync(SqlDataReader reader)
    {
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return new ResponseModel<OfficeModel>(404, "Office not found.");

        return new ResponseModel<OfficeModel>(200, "Office found.", MapOfficeFromReader(reader));
    }

    private static async Task<ResponseModel<IReadOnlyList<OfficeModel>>> HandleResponseWithOfficeListAsync(
        SqlDataReader reader)
    {
        var items = new List<OfficeModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapOfficeFromReader(reader) with
            {
                EmployeeCount = reader.GetInt32("EmployeeCount"),
                TotalGrossSalary = reader.GetDecimal("TotalGrossSalary")
            });

        return new ResponseModel<IReadOnlyList<OfficeModel>>(200, $"{items.Count} offices found.", items);
    }

    private static OfficeModel MapOfficeFromReader(SqlDataReader reader) => new()
    {
        OfficeId = reader.GetGuid("OfficeId"),
        Name = reader.GetString("Name"),
        City = reader.GetNullableString("City"),
        Country = reader.GetNullableString("Country")
    };
}
