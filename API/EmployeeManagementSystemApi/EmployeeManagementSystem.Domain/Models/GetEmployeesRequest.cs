namespace EmployeeManagementSystem.Domain.Models;

public class GetEmployeesRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }

    public string SortColumn { get; set; } = "name";

    public string SortDirection { get; set; } = "asc";
}

public class ExportEmployeesRequest
{
    public string? SearchTerm { get; set; }

    public string SortColumn { get; set; } = "name";

    public string SortDirection { get; set; } = "asc";
}
