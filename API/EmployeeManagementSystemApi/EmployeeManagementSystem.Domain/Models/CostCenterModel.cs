namespace EmployeeManagementSystem.Domain.Models;

public sealed record CostCenterModel
{
    public required Guid CostCenterId { get; init; }

    public required string Code { get; init; }

    public string? Name { get; init; }

    // Aggregates only CostCenter_List computes, so they're null — and omitted from the JSON —
    // on a single cost center.
    public int? EmployeeCount { get; init; }

    public decimal? TotalGrossSalary { get; init; }
}

// Request shape for POST create. No CostCenterId: the DB generates it.
public sealed class CreateCostCenterRequest
{
    public string? Code { get; set; }

    public string? Name { get; set; }
}

// Request shape for PATCH edit: an omitted field is left unchanged.
public sealed class UpdateCostCenterRequest
{
    public Guid CostCenterId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }
}
