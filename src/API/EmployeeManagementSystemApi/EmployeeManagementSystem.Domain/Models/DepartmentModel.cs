namespace EmployeeManagementSystem.Domain.Models;

public sealed record DepartmentModel
{
    public required Guid DepartmentId { get; init; }

    public required string Name { get; init; }

    public int? EmployeeCount { get; init; }

    public decimal? TotalGrossSalary { get; init; }
}
