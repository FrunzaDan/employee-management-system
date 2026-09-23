namespace EmployeeManagementSystem.Domain.Models;

public class EmployeeModel
{
    public Guid? Guid { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Msisdn { get; set; }

    public string? Email { get; set; }

    public EmployeeStatus? EmployeeStatus { get; set; }

    // UTC (DATETIME2 in the DB); serialized as ISO 8601 with a trailing "Z".
    public DateTime? CreationDate { get; set; }

    public DateTime? InteractionDate { get; set; }

    public Gender? Gender { get; set; }

    // Calendar dates, no time/zone component — serialized as "yyyy-MM-dd".
    public DateOnly? Birthdate { get; set; }

    public AddressModel? Address { get; set; }

    public DateOnly? HireDate { get; set; }

    public Guid? OfficeGuid { get; set; }

    public string? OfficeName { get; set; }

    public Guid? DepartmentGuid { get; set; }

    public string? DepartmentName { get; set; }

    public Guid? CostCenterGuid { get; set; }

    public string? CostCenterName { get; set; }

    // Read-only: the most recent EmployeeSalary entry for this employee
    // (see Employee_Get/Employee_List). Never set on create/edit — salary is
    // append-only history, changed only via EmployeeController's salaryHistory endpoint.
    public decimal? CurrentBruttoSalary { get; set; }
}

// Which Employee_Get parameter a free-text lookup maps to — detected from the
// search term's shape by EmployeeGetting, never supplied by the caller.
public enum EmployeeSearchOption
{
    None = 0,
    Guid = 1,
    Msisdn = 2,
    Email = 3
}

public class GetEmployeeRequest
{
    public EmployeeSearchOption SearchOption { get; set; }
    public string? SearchVariable { get; set; }
}
