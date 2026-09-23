using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeGetting
{
    private const int MaxPageSize = 100;

    // CSV export ignores paging (it's not a "current page" export) but still needs
    // a hard cap so an unfiltered export on a very large table can't balloon the
    // response — generous enough that no real local/demo dataset will ever hit it.
    private const int MaxExportRows = 5000;

    private static readonly string[] ValidSortColumns = ["name", "email", "msisdn"];
    private static readonly string[] ValidSortDirections = ["asc", "desc"];

    private readonly IDbUtils _dbUtils;

    public EmployeeGetting(IDbUtils dbUtils)
    {
        _dbUtils = dbUtils;
    }

    public async Task<ResponseModel<object>> GetEmployeeFunction(GetEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SearchVariable))
            return new ResponseModel<object>(404, "Search variable is required.");

        request.SearchOption = DetermineSearchOption(request.SearchVariable);

        if (request.SearchOption == EmployeeSearchOption.None)
            return new ResponseModel<object>(404,
                "No valid search variable was provided! It must be a GUID, MSISDN, or Email.");

        return await _dbUtils.GetEmployee(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetEmployeesFunction(GetEmployeesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PageNumber < 1)
            return new ResponseModel<object>(400, "Page number must be 1 or greater.");

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
            return new ResponseModel<object>(400, $"Page size must be between 1 and {MaxPageSize}.");

        var validationError = ValidateAndNormalizeSortAndSearch(request);
        if (validationError != null)
            return validationError;

        return await _dbUtils.GetEmployees(request, cancellationToken);
    }

    // Exports the full search/sort result (capped at MaxExportRows), not just one
    // page — it reuses usp_getEmployees via the same _dbUtils.GetEmployees call the
    // paginated endpoint uses, just with PageNumber/PageSize fixed internally, so the
    // filtering/sorting SQL stays in exactly one place.
    public async Task<ResponseModel<object>> GetEmployeesForExportFunction(ExportEmployeesRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagedRequest = new GetEmployeesRequest
        {
            PageNumber = 1,
            PageSize = MaxExportRows,
            SearchTerm = request.SearchTerm,
            SortColumn = request.SortColumn,
            SortDirection = request.SortDirection
        };

        var validationError = ValidateAndNormalizeSortAndSearch(pagedRequest);
        if (validationError != null)
            return validationError;

        var response = await _dbUtils.GetEmployees(pagedRequest, cancellationToken);
        if (response.Status != 200 || response.Data is not PagedResponse<EmployeeModel> paged)
            return response;

        var csv = EmployeeCsvExporter.ToCsv(paged.Items);
        return new ResponseModel<object>(200, $"{paged.Items.Count()} employees exported.", csv);
    }

    private static ResponseModel<object>? ValidateAndNormalizeSortAndSearch(GetEmployeesRequest request)
    {
        var sortColumn = request.SortColumn.Trim().ToLowerInvariant();
        if (!ValidSortColumns.Contains(sortColumn))
            return new ResponseModel<object>(400,
                $"Sort column must be one of: {string.Join(", ", ValidSortColumns)}.");

        var sortDirection = request.SortDirection.Trim().ToLowerInvariant();
        if (!ValidSortDirections.Contains(sortDirection))
            return new ResponseModel<object>(400,
                $"Sort direction must be one of: {string.Join(", ", ValidSortDirections)}.");

        request.SortColumn = sortColumn;
        request.SortDirection = sortDirection;
        request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();

        return null;
    }

    public async Task<ResponseModel<object>> GetEmployeeAuditLogFunction(Guid employeeGuid,
        CancellationToken cancellationToken = default)
    {
        if (employeeGuid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid employee GUID is required.");

        return await _dbUtils.GetEmployeeAuditLog(employeeGuid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetAllAuditLogFunction(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            return new ResponseModel<object>(400, "Page number must be 1 or greater.");

        if (pageSize < 1 || pageSize > MaxPageSize)
            return new ResponseModel<object>(400, $"Page size must be between 1 and {MaxPageSize}.");

        return await _dbUtils.GetAllEmployeeAuditLog(pageNumber, pageSize, cancellationToken);
    }

    private static EmployeeSearchOption DetermineSearchOption(string searchVariable)
    {
        return GuidValidation.ValidateGuid(searchVariable) ? EmployeeSearchOption.Guid :
            MsisdnValidation.ValidateMsisdn(searchVariable) ? EmployeeSearchOption.Msisdn :
            EmailValidation.ValidateEmail(searchVariable) && searchVariable.Length <= FieldLengthConstants.Email
                ? EmployeeSearchOption.Email :
            EmployeeSearchOption.None;
    }
}