namespace EmployeeManagementSystem.Domain.Models;

public sealed record EmployeeInsightsModel
{
    public required IReadOnlyList<EmployeeProfileModel> Employees { get; init; }
}

public sealed record EmployeeProfileModel
{
    public required EmployeeStatus Status { get; init; }

    public required Gender Gender { get; init; }

    public DateOnly? BirthDate { get; init; }

    public DateOnly? HireDate { get; init; }

    public string? DepartmentName { get; init; }

    public string? OfficeName { get; init; }

    public decimal? CurrentGrossSalary { get; init; }

    public IReadOnlyList<SalaryPointModel> SalaryHistory { get; init; } = [];
}

public sealed record SalaryPointModel
{
    public required DateOnly EffectiveDate { get; init; }

    public required decimal GrossSalary { get; init; }
}
