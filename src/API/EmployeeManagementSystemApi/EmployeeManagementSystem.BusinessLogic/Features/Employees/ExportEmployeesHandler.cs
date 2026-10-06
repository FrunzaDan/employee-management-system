using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Employees;

public class ExportEmployeesHandler(IEmployeeRepository employees)
{
    private const int MaxExportRows = 5000;

    public async Task<ResponseModel<string>> HandleAsync(ExportEmployeesRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagedRequest = new GetEmployeesRequest
        {
            PageNumber = 1,
            PageSize = MaxExportRows,
            SearchTerm = request.SearchTerm,
            SortColumn = request.SortColumn,
            SortDirection = request.SortDirection,
            OfficeId = request.OfficeId,
            DepartmentId = request.DepartmentId,
            CostCenterId = request.CostCenterId
        };

        var validationError = EmployeeListQuery.ValidateAndNormalizeSortAndSearch(pagedRequest);
        if (validationError != null)
            return new ResponseModel<string>(400, validationError);

        var response = await employees.GetEmployeesAsync(pagedRequest, cancellationToken);
        if (response is not { Status: 200, Data: { } paged })
            return new ResponseModel<string>(response.Status, response.ResponseMessage);

        if (paged.TotalItems > MaxExportRows)
            return new ResponseModel<string>(400,
                $"{paged.TotalItems} employees match, but an export is limited to {MaxExportRows}. Narrow the search and try again.");

        var csv = EmployeeCsvExporter.ToCsv(paged.Items);
        return new ResponseModel<string>(200, $"{paged.Items.Count} employees exported.", csv);
    }
}
