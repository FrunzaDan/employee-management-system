namespace EmployeeManagementSystem.Domain.Models;

public class SalaryHistoryEntry
{
    public string? SalaryGuid { get; set; }

    public string? EmployeeGuid { get; set; }

    public decimal? BruttoSalary { get; set; }

    public string? EffectiveDate { get; set; }

    public string? CreatedDate { get; set; }
}
