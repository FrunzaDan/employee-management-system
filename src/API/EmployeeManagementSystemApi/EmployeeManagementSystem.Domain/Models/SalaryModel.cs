namespace EmployeeManagementSystem.Domain.Models;

public sealed record SalaryModel
{
    public required int EmployeeSalaryId { get; init; }

    public required Guid EmployeeId { get; init; }

    public required decimal GrossSalary { get; init; }

    public required DateOnly EffectiveDate { get; init; }

    public required DateTime CreatedAt { get; init; }
}

public sealed class CreateSalaryRequest
{
    public Guid EmployeeId { get; set; }

    public decimal? GrossSalary { get; set; }

    public DateOnly? EffectiveDate { get; set; }
}
