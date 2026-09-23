namespace EmployeeManagementSystem.Domain.Models;

// One row of an employee's salary history (EmployeeSalary).
public sealed record SalaryModel
{
    public required int EmployeeSalaryId { get; init; }

    public required Guid EmployeeId { get; init; }

    public required decimal GrossSalary { get; init; }

    public required DateOnly EffectiveDate { get; init; }

    // UTC; server-set when the entry is recorded.
    public required DateTime CreatedAt { get; init; }
}

// Request shape for POST salary-history. Nullable members with no [Required], same reasoning
// as CreateEmployeeRequest — EmployeeSalary (BusinessLogic) validates every field. No
// EmployeeSalaryId: the DB generates it.
public sealed class CreateSalaryRequest
{
    public Guid EmployeeId { get; set; }

    public decimal? GrossSalary { get; set; }

    public DateOnly? EffectiveDate { get; set; }
}
