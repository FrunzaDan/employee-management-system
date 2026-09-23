namespace EmployeeManagementSystem.Domain.Models;

// A minimal projection of an employee for "which employees belong to this
// office/department/cost center" listings — deliberately not the full
// EmployeeModel (no address/hire-date/salary), which those queries don't join.
public class EmployeeSummary
{
    public Guid Guid { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public EmployeeStatus EmployeeStatus { get; set; }
}
