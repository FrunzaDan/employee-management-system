using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.DataAccess.Repositories;

/// <summary>Reads the employee lists that the office, department and cost center repositories all return.</summary>
internal static class EmployeeSummaryResults
{
    public static async Task<ResponseModel<IReadOnlyList<EmployeeSummaryModel>>> HandleResponseWithEmployeeSummaryListAsync(
        SqlDataReader reader)
    {
        var items = new List<EmployeeSummaryModel>();

        while (await reader.ReadAsync().ConfigureAwait(false))
            items.Add(MapEmployeeSummaryFromReader(reader));

        return new ResponseModel<IReadOnlyList<EmployeeSummaryModel>>(200, $"{items.Count} employees found.", items);
    }

    private static EmployeeSummaryModel MapEmployeeSummaryFromReader(SqlDataReader reader) => new()
    {
        EmployeeId = reader.GetGuid("EmployeeId"),
        FirstName = reader.GetString("FirstName"),
        LastName = reader.GetString("LastName"),
        Email = reader.GetString("Email"),
        Status = (EmployeeStatus)reader.GetInt16("StatusCode")
    };
}
