using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Employees;

/// <summary>The sort and search checks shared by the employee list and the CSV export.</summary>
internal static class EmployeeListQuery
{
    // Returns an error message, or null after trimming the search term in place.
    public static string? ValidateAndNormalizeSortAndSearch(GetEmployeesRequest request)
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
}
