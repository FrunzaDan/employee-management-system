namespace EmployeeManagementSystem.Domain.Models;

public sealed record CostCenterModel
{
    public required Guid CostCenterId { get; init; }

    public required string Code { get; init; }

    public string? Name { get; init; }

    public int? EmployeeCount { get; init; }

    public decimal? TotalGrossSalary { get; init; }
}
