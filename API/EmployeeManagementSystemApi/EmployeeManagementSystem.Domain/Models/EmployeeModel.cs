namespace EmployeeManagementSystem.Domain.Models;

public class EmployeeModel
{
    public string? Guid { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Msisdn { get; set; }

    public string? Email { get; set; }

    public int? EmployeeStatus { get; set; }

    public string? CreationDate { get; set; }

    public string? InteractionDate { get; set; }

    public int? Gender { get; set; }

    public string? Birthdate { get; set; }

    public AddressModel? Address { get; set; }

    public string? HireDate { get; set; }

    public string? OfficeGuid { get; set; }

    public string? OfficeName { get; set; }

    public string? DepartmentGuid { get; set; }

    public string? DepartmentName { get; set; }

    public string? CostCenterGuid { get; set; }

    public string? CostCenterName { get; set; }

    // Read-only: the most recent tbl_employee_salary_history entry for this employee
    // (see usp_getEmployee/usp_getEmployees). Never set on create/edit — salary is
    // append-only history, changed only via EmployeeController's salaryHistory endpoint.
    public decimal? CurrentBruttoSalary { get; set; }
}

public class GetEmployeeRequest
{
    public int SearchOption { get; set; }
    public string? SearchVariable { get; set; }
}