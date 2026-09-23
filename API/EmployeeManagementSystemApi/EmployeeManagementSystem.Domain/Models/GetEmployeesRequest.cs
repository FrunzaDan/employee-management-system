namespace EmployeeManagementSystem.Domain.Models;

public class GetEmployeesRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }

    public EmployeeSortColumn SortColumn { get; set; } = EmployeeSortColumn.Name;

    public SortDirection SortDirection { get; set; } = SortDirection.Asc;
}

public class ExportEmployeesRequest
{
    public string? SearchTerm { get; set; }

    public EmployeeSortColumn SortColumn { get; set; } = EmployeeSortColumn.Name;

    public SortDirection SortDirection { get; set; } = SortDirection.Asc;
}
