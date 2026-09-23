namespace EmployeeManagementSystem.Domain.Models;

public class SalaryHistoryEntry
{
    public Guid? SalaryGuid { get; set; }

    public Guid? EmployeeGuid { get; set; }

    public decimal? BruttoSalary { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    // UTC; server-set when the entry is recorded.
    public DateTime? CreatedDate { get; set; }
}
