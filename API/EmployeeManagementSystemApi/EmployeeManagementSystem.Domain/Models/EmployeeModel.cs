namespace EmployeeManagementSystem.Domain.Models;

public sealed record EmployeeModel
{
    public required Guid EmployeeId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string PhoneNumber { get; init; }

    public required string Email { get; init; }

    public required EmployeeStatus Status { get; init; }

    public required DateTime CreatedAt { get; init; }

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

public sealed class CreateEmployeeRequest
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public Gender? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public EmployeeStatus? Status { get; set; }

    public AddressRequest? Address { get; set; }

    public DateOnly? HireDate { get; set; }

    public Guid? OfficeId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? CostCenterId { get; set; }
}

public sealed class UpdateEmployeeRequest
{
    public Guid EmployeeId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public Gender? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public AddressRequest? Address { get; set; }

    public DateOnly? HireDate { get; set; }

    public Guid? OfficeId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? CostCenterId { get; set; }
}

public sealed record EmployeeLookup(Guid? EmployeeId = null, string? PhoneNumber = null, string? Email = null);

public sealed record EmployeeSummaryModel
{
    public required Guid EmployeeId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public required EmployeeStatus Status { get; init; }
}
