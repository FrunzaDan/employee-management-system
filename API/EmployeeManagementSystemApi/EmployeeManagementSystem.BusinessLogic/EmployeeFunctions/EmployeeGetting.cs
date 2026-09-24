using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeGetting(IDbUtils dbUtils)
{
    private const int MaxPageSize = 100;

    // CSV export ignores paging (it's not a "current page" export) but still needs
    // a hard cap so an unfiltered export on a very large table can't balloon the
    // response — generous enough that no real local/demo dataset will ever hit it.
    private const int MaxExportRows = 5000;

    public async Task<ResponseModel<EmployeeModel>> GetEmployeeAsync(string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new ResponseModel<EmployeeModel>(400, "Search variable cannot be null or empty.");

        var lookup = DetermineLookup(searchTerm.Trim());
        if (lookup is null)
            return new ResponseModel<EmployeeModel>(404,
                "No valid search variable was provided! It must be a employee ID, phone number, or email.");

        return await dbUtils.GetEmployeeAsync(lookup, cancellationToken);
    }

    public async Task<ResponseModel<PagedResponse<EmployeeModel>>> GetEmployeesAsync(GetEmployeesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PageNumber < 1)
            return new ResponseModel<PagedResponse<EmployeeModel>>(400, "Page number must be 1 or greater.");

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
            return new ResponseModel<PagedResponse<EmployeeModel>>(400,
                $"Page size must be between 1 and {MaxPageSize}.");

        var validationError = ValidateAndNormalizeSortAndSearch(request);
        if (validationError != null)
            return new ResponseModel<PagedResponse<EmployeeModel>>(400, validationError);

        return await dbUtils.GetEmployeesAsync(request, cancellationToken);
    }

    // Exports the full search/sort result (capped at MaxExportRows), not just one
    // page — it reuses Employee_List via the same dbUtils.GetEmployees call the
    // paginated endpoint uses, just with PageNumber/PageSize fixed internally, so the
    // filtering/sorting SQL stays in exactly one place.
    public async Task<ResponseModel<string>> GetEmployeesForExportAsync(ExportEmployeesRequest request,
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
            return new ResponseModel<string>(400, validationError);

        var response = await dbUtils.GetEmployeesAsync(pagedRequest, cancellationToken);
        if (response is not { Status: 200, Data: { } paged })
            return new ResponseModel<string>(response.Status, response.ResponseMessage);

        var csv = EmployeeCsvExporter.ToCsv(paged.Items);
        return new ResponseModel<string>(200, $"{paged.Items.Count} employees exported.", csv);
    }

    // The enums can only hold an undefined value if one was forced in (e.g. "?sortColumn=7"
    // binds to (EmployeeSortColumn)7), so this is a backstop, not the primary check.
    private static string? ValidateAndNormalizeSortAndSearch(GetEmployeesRequest request)
    {
        if (!Enum.IsDefined(request.SortColumn))
            return $"Sort column must be one of: {string.Join(", ", Enum.GetNames<EmployeeSortColumn>())}.";

        if (!Enum.IsDefined(request.SortDirection))
            return $"Sort direction must be one of: {string.Join(", ", Enum.GetNames<SortDirection>())}.";

        request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();

        if (request.SearchTerm?.Length > FieldLengthConstants.SearchTerm)
            return "Search term is too long.";

        return null;
    }

    public async Task<ResponseModel<IReadOnlyList<AuditLogEntry>>> GetEmployeeAuditLogAsync(Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<AuditLogEntry>>(400, "A valid employee ID is required.");

        return await dbUtils.GetEmployeeAuditLogAsync(employeeId, cancellationToken);
    }

    public async Task<ResponseModel<PagedResponse<GlobalAuditLogEntry>>> GetAllEmployeeAuditLogAsync(int pageNumber,
        int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            return new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(400, "Page number must be 1 or greater.");

        if (pageSize < 1 || pageSize > MaxPageSize)
            return new ResponseModel<PagedResponse<GlobalAuditLogEntry>>(400,
                $"Page size must be between 1 and {MaxPageSize}.");

        return await dbUtils.GetAllEmployeeAuditLogAsync(pageNumber, pageSize, cancellationToken);
    }

    // Picks the one key Employee_Get should seek on, from the search term's shape: GUID
    // (any format Guid.TryParse accepts — braces, upper case, no hyphens), then phone number, then
    // email. Null when it's none of the three.
    private static EmployeeLookup? DetermineLookup(string searchTerm)
    {
        if (Guid.TryParse(searchTerm, out var employeeId))
            return new EmployeeLookup(EmployeeId: employeeId);

        if (PhoneNumberValidation.ValidatePhoneNumber(searchTerm))
            return new EmployeeLookup(PhoneNumber: searchTerm);

        if (EmailValidation.ValidateEmail(searchTerm) && searchTerm.Length <= FieldLengthConstants.Email)
            return new EmployeeLookup(Email: searchTerm);

        return null;
    }
}
