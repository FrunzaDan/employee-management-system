using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Contracts;

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

public sealed class AddressRequest
{
    public string? Country { get; set; }
    public string? County { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string? StreetNumber { get; set; }
}
