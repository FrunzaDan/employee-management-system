namespace EmployeeManagementSystem.Domain.Models;

public sealed record DepartmentModel
{
    public required Guid DepartmentId { get; init; }

    public required string Name { get; init; }

    // Aggregates only Department_List computes, so they're null — and omitted from the JSON —
    // on a single department.
    public int? EmployeeCount { get; init; }

    public decimal? TotalGrossSalary { get; init; }
}

// Request shape for POST create. No DepartmentId: the DB generates it.
public sealed class CreateDepartmentRequest
{
    public string? Name { get; set; }
}

// Request shape for PATCH edit: an omitted field is left unchanged.
public sealed class UpdateDepartmentRequest
{
    public Guid DepartmentId { get; set; }

    public string? Name { get; set; }
}
