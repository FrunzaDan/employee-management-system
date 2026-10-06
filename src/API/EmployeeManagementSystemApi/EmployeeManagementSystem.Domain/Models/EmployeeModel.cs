namespace EmployeeManagementSystem.Domain.Models;

public sealed record EmployeeModel
{
    public required Guid EmployeeId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string PhoneNumber { get; init; }

    public required string Email { get; init; }

    public required EmployeeStatus Status { get; init; }

    public required DateTime AccountCreatedAt { get; init; }

    public required DateTime LastInteractionAt { get; init; }

    public required Gender Gender { get; init; }

    public DateOnly? BirthDate { get; init; }

    public required AddressModel Address { get; init; }

    public DateOnly? HireDate { get; init; }

    public Guid? OfficeId { get; init; }

    public string? OfficeName { get; init; }

    public Guid? DepartmentId { get; init; }

    public string? DepartmentName { get; init; }

    public Guid? CostCenterId { get; init; }

    public string? CostCenterName { get; init; }

    public decimal? CurrentGrossSalary { get; init; }
}

public sealed record EmployeeSummaryModel
{
    public required Guid EmployeeId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public required EmployeeStatus Status { get; init; }
}
